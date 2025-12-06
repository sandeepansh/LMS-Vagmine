namespace TMS.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; }
        public int TotalCourses { get; set; }
        public int TotalExams { get; set; }
        public int TotalResults { get; set; }
        public double PassingRate { get; set; }

        public List<string> TodayExams { get; set; } = new();
        public List<string> TodayVirtualClasses { get; set; } = new();
    }
}
