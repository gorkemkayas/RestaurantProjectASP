using System.ComponentModel.DataAnnotations;

namespace deneme.Models
{
    public class Staff
    {
        public int StaffId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public int RoleId { get; set; }

        [Required]
        public string Phone { get; set; } = null!;

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
