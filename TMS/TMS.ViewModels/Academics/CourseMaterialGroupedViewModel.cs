using System.Collections.Generic;
using TMS.ViewModels.Masters;

namespace TMS.ViewModels.Academics
{
    //public class CourseMaterialGroupedViewModel
    //{
    //    public CourseMasterViewModel Course { get; set; } = new();
    //    public Dictionary<string, List<LectureMaterialViewModel>> GroupedMaterials { get; set; } = new();
    //}
    public class CourseMaterialGroupedViewModel
    {
        public CourseMasterViewModel Course { get; set; } = new();
        public List<QuadrantMaterialGroupViewModel> GroupedMaterials { get; set; } = new();
    }
    public class QuadrantMaterialGroupViewModel
    {
        public int QuadrantId { get; set; }
        public string QuadrantName { get; set; } = string.Empty;
        public List<LectureMaterialViewModel> Materials { get; set; } = new();
        public bool HasAttemptedQuiz { get; set; }
    }




}
