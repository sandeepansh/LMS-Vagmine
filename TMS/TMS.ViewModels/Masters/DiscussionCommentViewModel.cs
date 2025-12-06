using TMS;
using System;
using System.ComponentModel.DataAnnotations;
using TMS.Common;

namespace TMS.ViewModels.Masters
{
    public class DiscussionCommentViewModel : BaseViewModel
    {
        [MapToDTO, Required, Display(Name = "Thread")]
        public int ThreadId { get; set; }

        [MapToDTO, Required, Display(Name = "User")]
        public int UserId { get; set; }

        [MapToDTO, Required, Display(Name = "Comment Text"), MaxLength(1000)]
        public string CommentText { get; set; } = default!;

        [MapToDTO, Display(Name = "Commented On")]
        public DateTime CommentedOn { get; set; } = DateTime.UtcNow;

        public virtual DiscussionThreadViewModel? Thread { get; set; }
        public virtual UserViewModel? User { get; set; }
    }
}
