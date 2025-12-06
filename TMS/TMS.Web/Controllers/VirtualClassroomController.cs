using Microsoft.AspNetCore.Mvc;
// hypothetical service layer
using System.Linq;
using TMS.Common;
using TMS.Models.Academics;
using TMS.Models.Masters;
using TMS.Repository.Managers;
using TMS.ViewModels.Academics;
using TMS.ViewModels.Masters;

namespace TMS.Web.Controllers
{
    public class VirtualClassroomController : BaseController
    {
        private readonly IMasterBaseManager<CourseEnrollmentViewModel> _enrollmentManager;
        private readonly IMasterBaseManager<CourseMeetingViewModel> _meetingManager;

        public VirtualClassroomController(
            IMasterBaseManager<CourseEnrollmentViewModel> enrollmentManager,
            IMasterBaseManager<CourseMeetingViewModel> meetingManager)
        {
            _enrollmentManager = enrollmentManager;
            _meetingManager = meetingManager;

        }

        public async Task<IActionResult> Index()
        {
            int studentId = GetUserId(); // ✅ Logged-in user ID

            // 1️⃣ Get all enrolled courses for the logged-in student
            var enrollments = await _enrollmentManager.GetAsync(null, x => x.IsActive);
            var studentEnrollments = enrollments.Where(e => e.StudentId == studentId)
                .ToList();
            var courseIds = studentEnrollments.Select(e => e.CourseId).ToList();

            // 2️⃣ Get all meetings for those courses
            var allMeetings = await _meetingManager.GetAsync(null,t => t.IsActive);
           var meetingsForStudent = allMeetings
                .Where(m => courseIds.Contains(m.CourseId))
                .ToList();

            var now = DateTime.UtcNow;
            var upcoming = meetingsForStudent
                .Where(m => m.ScheduledAt >= now)
                .OrderBy(m => m.ScheduledAt)
                .ToList();

            var past = meetingsForStudent
                .Where(m => m.ScheduledAt < now)
                .OrderByDescending(m => m.ScheduledAt)
                .ToList();

            var model = new VirtualClassroomViewModel
            {
                UpcomingSessions = upcoming,
                PastSessions = past
            };

            return View(model);
        }
    }
}
