using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS.Models.Masters
{
    [Table(nameof(LectureMaterial))]
    public class LectureMaterial : BaseModel
    {

        [Required]
        public int CourseId { get; set; }   

        [ForeignKey(nameof(CourseId))]
        public virtual CourseMaster? Course { get; set; }  

        public int CourseQuadrantId { get; set; }

        [ForeignKey(nameof(CourseQuadrantId))]
        public virtual CourseQuadrantMaster? CourseQuadrant { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = default!;

        [Required, StringLength(20)]
        public string MaterialType { get; set; } = "PDF"; // "PDF" or "Video"

     
        [Required]
        public string FilePath { get; set; } = default!;

        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
    }
}
