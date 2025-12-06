using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Models.Masters;

namespace TMS.Models.Account
{
    [Table(nameof(UserMaster))]
    public class UserMaster : BaseMasterModel
    {
        [NotMapped]
        public override string? Description { get; set; }
        [Required]
        [StringLength(100)]
        public string? Email { get; set; }
        [Required]
        [StringLength(100)]
        public string? Password { get; set; }
        public int? RoleId { get; set; }

       
        public int? ContactNo{  get; set; }

		[ForeignKey(nameof(RoleId))]
        public virtual RoleMaster? Role { get; set; }
        public virtual List<UserRoles>? UserRoles { get; set; }

       
	}
}
