using TMS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TMS.Common;
using TMS.Models.Masters;
using TMS.Models.Account;

namespace TMS.ViewModels.Masters
{
    public class CourseEnrollmentViewModel : BaseViewModel
    {
        [MapToDTO, Required, Display(Name = "Course")]
        public int CourseId { get; set; }

        [MapToDTO, Required, Display(Name = "Student")]
        public int StudentId { get; set; }

        [MapToDTO, Display(Name = "Enrollment Date")]
        public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;
        public UserMaster? Student { get; set; }
        public CourseMasterViewModel Course { get; set; } = new();
        public List<UserViewModel> AvailableStudents { get; set; } = new();
        public List<int> SelectedStudentIds { get; set; } = new();
    }
}
