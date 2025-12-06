using TMS;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Common;

namespace TMS.ViewModels.Masters
{
    public class LectureMaterialViewModel : BaseViewModel
    {
        [MapToDTO,Required(ErrorMessage = "Course is required")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }
        [MapToDTO, Required, Display(Name = "Quadrant")]
        public int CourseQuadrantId { get; set; }

        [MapToDTO, Required, Display(Name = "Material Type")]
        public string? MaterialType { get; set; }  // e.g., "Video", "PDF"

        [MapToDTO, Required, Display(Name = "File Path")]
        public string? FilePath { get; set; }

        [MapToDTO, Display(Name = "Title"), MaxLength(200)]
        public string? Title { get; set; }

        [MapToDTO, Display(Name = "Description"), MaxLength(500)]
        public string? Description { get; set; }

        public CourseMasterViewModel? Course { get; set; }
        public CourseQuadrantViewModel? CourseQuadrant { get; set; }
        [Display(Name = "Content URL")]
        public string? ContentPath { get; set; }
        [NotMapped]
        public bool MeetingExists { get; set; }
    }
}
