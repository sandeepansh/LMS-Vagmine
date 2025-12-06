using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMS.Repository.Managers;
using TMS.Common;
using TMS.ViewModels.Academics;
using TMS.ViewModels.Masters;

namespace TMS.Web.Controllers
{
    public class CalendarController : BaseController
    {
        private readonly IMasterBaseManager<CourseEnrollmentViewModel> _enrollmentManager;
        private readonly IMasterBaseManager<CourseMeetingViewModel> _meetingManager;
       

        public CalendarController(             
            IMasterBaseManager<CourseMeetingViewModel> meetingManager,
            IMasterBaseManager<CourseEnrollmentViewModel> enrollmentManager)
        {
            _enrollmentManager = enrollmentManager;
            _meetingManager = meetingManager;

        }

        public async Task<IActionResult> Index()
        {
            // ✅ 1. Get all courses in which the logged-in student is enrolled
            int studentId = GetUserId(); // ✅ Logged-in user ID

            // 1️⃣ Get all enrolled courses for the logged-in student
            var enrollments = await _enrollmentManager.GetAsync(null, x => x.IsActive);
            var studentEnrollments = enrollments.Where(e => e.StudentId == studentId)
                .ToList();
            var courseIds = studentEnrollments.Select(e => e.CourseId).ToList();
            if (!courseIds.Any())
            {
                ViewBag.Message = "You are not enrolled in any course.";
                return View(new List<VirtualClassroomViewModel>());
            }

            // ✅ 2. Get all meetings for those enrolled courses
            // 2️⃣ Get all meetings for those courses
            var allMeetings = await _meetingManager.GetAsync(null, t => t.IsActive);
            var meetingsForStudent = allMeetings
                 .Where(m => courseIds.Contains(m.CourseId))
                 .ToList();
            // ✅ 3. Optional: separate upcoming vs past
            var now = DateTime.UtcNow;
            var upcoming = meetingsForStudent.Where(m => m.ScheduledAt >= now)
                                      .OrderBy(m => m.ScheduledAt)
                                      .ToList();

            var past = meetingsForStudent.Where(m => m.ScheduledAt < now)
                                  .OrderByDescending(m => m.ScheduledAt)
                                  .ToList();

            // ✅ 4. Combine (if needed)
            var model = upcoming.Concat(past).ToList();

            return View(model);
        }
    }
}
