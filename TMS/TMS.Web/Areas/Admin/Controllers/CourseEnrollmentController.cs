using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMS.Repository.Managers;
using TMS.ViewModels;
using TMS.ViewModels.Masters;
using TMS.Web.Controllers;

namespace TMS.Web.Areas.Admin.Controllers
{
    [ValidateFormAccess(Common.FormDefination.Enrollment)]
    [Area("Admin")]
    public class CourseEnrollmentController : BaseController
    {
        private readonly IMasterBaseManager<CourseEnrollmentViewModel> _enrollmentManager;
        private readonly IMasterManager<CourseMasterViewModel> _courseManager;
        private readonly IMasterManager<UserViewModel> _userManager;

        public CourseEnrollmentController(
            IMasterBaseManager<CourseEnrollmentViewModel> enrollmentManager,
            IMasterManager<CourseMasterViewModel> courseManager,
            IMasterManager<UserViewModel> userManager)
        {
            _enrollmentManager = enrollmentManager;
            _courseManager = courseManager;
            _userManager = userManager;
        }
 
     
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Course Enrollment Management";

            var courses = await _courseManager.GetAsync(new[] { "Semester", "CourseCategory" }, c => c.IsActive);
            var allEnrollments = await _enrollmentManager.GetAsync(new[] { "Course", "Student" });

            foreach (var course in courses)
            {
                course.EnrolledStudents = allEnrollments
                    .Where(e => e.CourseId == course.Id)
                    .Select(e => e.Student)
                    .ToList();
            }

            return View(courses);
        }

        // =============================
        // GET: ManageMapping - Enroll multiple students to a course
        // =============================
        [HttpGet]
        public async Task<IActionResult> ManageMapping(int courseId)
        {
            // 1️⃣ Fetch all students with the "Student" role
            var students = await _userManager.GetAsync(new[] { "Role" },
                u => u.IsActive && u.Role != null && u.Role.Name == "Student");

            // 2️⃣ Get all existing enrollments for this course
            var existingEnrollments = await _enrollmentManager.GetAsync(null, e => e.CourseId == courseId);
            var selectedIds = existingEnrollments.Select(e => e.StudentId).ToList();

            // 3️⃣ Build the ViewModel
            var viewModel = new CourseEnrollmentViewModel
            {
                CourseId = courseId,
                SelectedStudentIds = selectedIds
            };

            // 4️⃣ Fetch course info
            var course = await _courseManager.GetAsync(courseId);
            ViewBag.CourseName = course?.Name ?? "Unknown Course";

            // 5️⃣ Prepare the select list for checkboxes
            ViewBag.StudentList = students.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name,
                Selected = selectedIds.Contains(s.Id)
            }).ToList();

            // 6️⃣ Pass full student objects for displaying extra info in the table
            ViewBag.StudentDetails = students;

            // 7️⃣ Return partial view
            return PartialView("_ManageMapping", viewModel);
        }


        // =============================
        // POST: SaveMapping - Save selected students
        // =============================
        [HttpPost]
        public async Task<IActionResult> SaveMapping(CourseEnrollmentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = await _enrollmentManager.GetAsync(null, e => e.CourseId == model.CourseId);
            foreach (var enrollment in existing)
            {
                await _enrollmentManager.DeleteAsync(enrollment.Id);
            }

            if (model.SelectedStudentIds != null && model.SelectedStudentIds.Any())
            {
                foreach (var studentId in model.SelectedStudentIds)
                {
                    await _enrollmentManager.AddUpdateAsync(new CourseEnrollmentViewModel
                    {
                        CourseId = model.CourseId,
                        StudentId = studentId,
                        EnrolledOn = DateTime.UtcNow,
                        IsActive = true
                    }, GetUserId());
                }
            }
            SetApplicationResult(true, "Saved successfully.");
            return RedirectToAction(nameof(Index));
           
        }

        // =============================
        // GET: ViewMapping - View all enrolled students
        // =============================
        [HttpGet]
        public async Task<IActionResult> ViewMapping(int courseId)
        {
            var enrollments = await _enrollmentManager.GetAsync(new[] { "Student" }, e => e.CourseId == courseId);
            ViewBag.CourseName = (await _courseManager.GetAsync(courseId))?.Name ?? "Unknown Course";
            return PartialView("_ViewMapping", enrollments);
        }
    }
}
