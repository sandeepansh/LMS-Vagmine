using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using TMS.Models.Academics;
using TMS.Repository.Managers;
using TMS.Utilities;
using TMS.ViewModels;
using TMS.ViewModels.Academics;
using TMS.ViewModels.Masters;
using TMS.Web.Controllers;
using TMS.Web.Models;

namespace TMS.Web.Areas.Admin.Controllers
{
    [ValidateFormAccess(Common.FormDefination.LectureMaterial)]
    [Area("Admin")]
    public class LectureMaterialController : BaseController
    {
        private readonly IMasterBaseManager<LectureMaterialViewModel> _lectureMaterialManager;
        private readonly IMasterBaseManager<CourseMeetingViewModel> _meetingManager;
        private readonly IMasterManager<CourseMasterViewModel> _courseManager;
        private readonly IMasterBaseManager<CourseQuadrantViewModel> _quadrantManager;
        private readonly IMasterBaseManager<CourseFacultyMapViewModel> _courseFacultyMapManager;
        private readonly IWebHostEnvironment _env;
        private readonly ZoomSettings _zoomSettings;

        public LectureMaterialController(
     IMasterBaseManager<LectureMaterialViewModel> lectureMaterialManager,
     IMasterManager<CourseMasterViewModel> courseManager,
     IMasterBaseManager<CourseQuadrantViewModel> quadrantManager,
     IMasterBaseManager<CourseFacultyMapViewModel> courseFacultyMapManager,
     IWebHostEnvironment env, IOptions<ZoomSettings> options, IMasterBaseManager<CourseMeetingViewModel> meetingManager)
        {
            _lectureMaterialManager = lectureMaterialManager;
            _courseManager = courseManager;
            _quadrantManager = quadrantManager;
            _courseFacultyMapManager = courseFacultyMapManager;
            _env = env;
            _zoomSettings = options.Value;
            _meetingManager = meetingManager;

        }


        // GET: Admin/LectureMaterial
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Course Content";

            int userId = GetUserId();
            string? userRole = User.FindFirstValue(ClaimTypes.Role);

            IEnumerable<LectureMaterialViewModel> list;

            if (userRole?.Equals("Faculty", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Get all course mappings for this faculty
                var mappings = await _courseFacultyMapManager.GetAsync(new[] { "Course" }, x => x.FacultyId == userId);
                var courseIds = mappings.Select(m => m.CourseId).Distinct().ToList();

                // Show lecture materials only for those courses
                list = await _lectureMaterialManager.GetAsync(new[] { "Course", "CourseQuadrant" }, x => courseIds.Contains(x.CourseId));
            }
            else
            {
                // Admins or others can see everything
                list = await _lectureMaterialManager.GetAsync(new[] { "Course", "CourseQuadrant" });
            }
            if(list != null && list.Any())
            {
                foreach(var item in list)
                {
                    var meetings = await _meetingManager.GetAsync(
                        null,
                        m => m.CourseId == item.CourseId && m.CourseQuadrantId == item.CourseQuadrantId && m.IsActive
                    );
                    item.MeetingExists = meetings != null && meetings.Any();
                }
            }
            return View(list);
        }


        // GET: Admin/LectureMaterial/Item/{id?}
        [HttpGet]
        public async Task<IActionResult> Item(string iId)
        {
            LectureMaterialViewModel model = new();



            if (!string.IsNullOrEmpty(iId))
            {
                iId = iId.Decrypt(true);
                if (string.IsNullOrWhiteSpace(iId))
                    return NotFound();
                int materialId = Convert.ToInt32(iId);
                var existing = await _lectureMaterialManager.GetAsync(materialId, new[] { "Course", "CourseQuadrant" });

                if (existing != null)
                    model = existing;
                if (model.MaterialType == "VideoURL" && !string.IsNullOrEmpty(model.FilePath))
                {
                    model.ContentPath = model.FilePath;
                }
            }

            await SetDropdowns(model);
            return View(model);
        }

        // POST: Admin/LectureMaterial/Item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Item(LectureMaterialViewModel model, IFormFile? uploadFile)
        {
            int userId = GetUserId();
            string? userRole = GetUserRole();

            //if (!ModelState.IsValid)
            //{
            //    await SetDropdowns(model);
            //    return View(model);
            //}

            // ✅ Validate duplicate record
            var isexist = await ValidateModel(model);
            if (isexist)
            {
                await SetDropdowns(model);
                return View(model);
            }

            // ✅ Handle based on MaterialType
            if (model.MaterialType == "Video" || model.MaterialType == "Document")
            {
                if (uploadFile != null && uploadFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "lectures");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(uploadFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadFile.CopyToAsync(fileStream);
                    }

                    // ✅ Store relative path for file
                    model.FilePath = $"/uploads/lectures/{uniqueFileName}";
                }
                else if (model.Id == 0) // for new record, ensure file required
                {
                    ModelState.AddModelError("FilePath", "Please upload a file.");
                    await SetDropdowns(model);
                    return View(model);
                }
            }
            else if (model.MaterialType == "VideoURL")
            {
                model.FilePath = model.ContentPath;
                // ✅ Ensure URL is entered
                if (string.IsNullOrWhiteSpace(model.FilePath))
                {
                    ModelState.AddModelError("FilePath", "Please enter a valid video URL.");
                    await SetDropdowns(model);
                    return View(model);
                }

                // ✅ For URLs, no file upload is expected — leave FilePath as-is
                // Ensure URL normalization (optional)
                model.FilePath = model.FilePath.Trim();
            }

            // ✅ Save/Update
            bool result = await _lectureMaterialManager.AddUpdateAsync(model, userId);
            if (result)
            {
                TempData["Success"] = "Lecture material saved successfully.";
                SetApplicationResult(true, "Lecture material saved successfully.");
                return RedirectToAction(nameof(Index));
            }

            SetApplicationResult(false, "Error while saving data.");
            await SetDropdowns(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleMeeting(int courseId, int quadrantId, DateTime startTime, string MeetingTitle, int duration)
        {
            var zoomService = new TMS.Utilities.ZoomService(_zoomSettings);

            var meetingUrl = await zoomService.CreateMeetingAsync(
                $"Course {MeetingTitle} - Virtual Session",
                startTime,
                duration// duration
            );

            var meeting = new CourseMeetingViewModel
            {
                MeetingTitle = MeetingTitle,
                CourseId = courseId,
                CourseQuadrantId = quadrantId,
                ScheduledAt = startTime,
                MeetingUrl = meetingUrl,
                CreatedBy = GetUserId()
            };

            var res = await _meetingManager.AddUpdateAsync(meeting, GetUserId());


            SetApplicationResult(true, "Virtual Class scheduled successfully.");
            return RedirectToAction("Index");
        }
        private async Task SetDropdowns(LectureMaterialViewModel model)
        {
            int userId = GetUserId();
            string? userRole = GetUserRole();
            userRole = User.FindFirstValue("role");

            List<CourseMasterViewModel> courses;

            if (userRole?.Equals("Faculty", StringComparison.OrdinalIgnoreCase) == true)
            {
                // 1️⃣ Get mapped courses for this faculty — note the parameter order
                var mappings = await _courseFacultyMapManager.GetAsync(new[] { "Course" }, x => x.FacultyId == userId);

                var courseIds = mappings.Select(m => m.CourseId).Distinct().ToList();

                // 2️⃣ Filter courses based on mapped CourseIds
                var allCourses = await _courseManager.GetAsync(null, t => t.IsActive);

                ViewBag.CourseId = allCourses;
            }
            else
            {

                var allCourses = await _courseManager.GetAsync(null, t => t.IsActive);

                courses = allCourses.ToList();
                ViewBag.CourseId = courses;
            }





            var quadrants = await _quadrantManager.GetAsync(null, t => t.IsActive || t.Id == model.CourseQuadrantId);
            ViewBag.QuadrantId = quadrants;
        }
        private async Task<bool> ValidateModel(LectureMaterialViewModel model)
        {

            var resultIdCheck = await _lectureMaterialManager.CheckExpression(t => (model.Id == 0 || t.Id != model.Id)
&& t.CourseId == model.CourseId && t.CourseQuadrantId == model.CourseQuadrantId && t.Title == model.Title);

            if (resultIdCheck)
                ModelState.AddModelError("Title", "Title already in use");
            return resultIdCheck;
        }

        [HttpGet]
        public async Task<IActionResult> GetMeetingDetails(int courseId, int quadrantId)
        {
            // Fetch all active meetings for the given course & quadrant
            var meetings = await _meetingManager.GetAsync(
                null,
                m => m.CourseId == courseId && m.CourseQuadrantId == quadrantId && m.IsActive
            );

            if (meetings != null && meetings.Any())
            {
                var result = meetings
                    .OrderByDescending(m => m.ScheduledAt)
                    .Select(m => new
                    {
                        id = m.Id,
                        title = m.MeetingTitle,
                        startTime = m.ScheduledAt.ToString("yyyy-MM-ddTHH:mm"),
                        duration = m.Duration,
                        joinUrl = m.MeetingUrl
                    })
                    .ToList();

                return Json(new
                {
                    hasMeeting = true,
                    meetings = result
                });
            }

            return Json(new { hasMeeting = false });
        }


    }
}
