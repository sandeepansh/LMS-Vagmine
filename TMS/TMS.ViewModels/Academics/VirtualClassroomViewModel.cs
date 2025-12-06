using System.Collections.Generic;
using TMS.ViewModels; // adjust if CourseMeetingViewModel lives in another namespace
// using TMS.ViewModels.Academics; // uncomment/adjust if needed

namespace TMS.ViewModels.Academics
{
    public class VirtualClassroomViewModel
    {
        /// <summary>
        /// Upcoming sessions (ScheduledAt >= now)
        /// </summary>
        public List<CourseMeetingViewModel> UpcomingSessions { get; set; } = new();

        /// <summary>
        /// Past sessions (ScheduledAt < now)
        /// </summary>
        public List<CourseMeetingViewModel> PastSessions { get; set; } = new();

        /// <summary>
        /// Optional: search term persisted (if you want to keep search server-side)
        /// </summary>
        public string? SearchTerm { get; set; }
    }
}
