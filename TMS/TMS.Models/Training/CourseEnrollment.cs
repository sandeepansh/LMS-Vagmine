using System;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Models.Account;
using TMS.Models.Masters;
 

namespace TMS.Models.Training
{
    [Table(nameof(CourseEnrollment))]
    public class CourseEnrollment : BaseModel
    {
        public int CourseId { get; set; }
        [ForeignKey(nameof(CourseId))]
        public virtual CourseMaster? Course { get; set; }

        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public virtual UserMaster? Student { get; set; }

        public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;
    }
}
