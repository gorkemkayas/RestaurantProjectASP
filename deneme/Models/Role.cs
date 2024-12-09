using System.ComponentModel.DataAnnotations;

namespace deneme.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        [Required]
        public string RoleName { get; set; } = null!;

    }
}
