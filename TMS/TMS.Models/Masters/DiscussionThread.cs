using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Models.Account;


namespace TMS.Models.Masters
{
    [Table(nameof(DiscussionThread))]
    public class DiscussionThread : BaseModel
    {
        public int CourseQuadrantId { get; set; }
        [ForeignKey(nameof(CourseQuadrantId))]
        public virtual CourseQuadrantMaster? CourseQuadrant { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = default!;

        public int CreatedByUserId { get; set; }
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual UserMaster? CreatedByUser { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public virtual List<DiscussionComment>? Comments { get; set; }
    }

    [Table(nameof(DiscussionComment))]
    public class DiscussionComment : BaseModel
    {
        public int ThreadId { get; set; }
        [ForeignKey(nameof(ThreadId))]
        public virtual DiscussionThread? Thread { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual UserMaster? User { get; set; }

        [Required]
        public string CommentText { get; set; } = default!;

        public DateTime CommentedOn { get; set; } = DateTime.UtcNow;
    }
}
