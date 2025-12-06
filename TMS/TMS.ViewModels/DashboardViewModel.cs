namespace TMS.Web.Models.ViewModels
{
    public class CourseCategoryStatViewModel
    {
        public string CategoryName { get; set; }
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalMaterials { get; set; }
        public int TotalStudents { get; set; }
        public List<CourseCategoryStatViewModel> CourseCategoryStats { get; set; }
    }
}
