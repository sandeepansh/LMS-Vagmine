using TMS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TMS.Common;

namespace TMS.ViewModels.Masters
{
    public class DiscussionThreadViewModel : BaseViewModel
    {
        [MapToDTO, Required, Display(Name = "Course Quadrant")]
        public int CourseQuadrantId { get; set; }

        [MapToDTO, Required, MaxLength(100), Display(Name = "Title")]
        public string Title { get; set; } = default!;

        [MapToDTO, Required, Display(Name = "Created By User")]
        public int CreatedByUserId { get; set; }

        [MapToDTO, Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public virtual CourseQuadrantViewModel? CourseQuadrant { get; set; }
        public virtual UserViewModel? CreatedByUser { get; set; }
        public virtual List<DiscussionCommentViewModel> Comments { get; set; } = new();
    }
}
