using System.ComponentModel.DataAnnotations;

namespace deneme.Models
{
    public class Tables
    {
        [Required]
        public int? TableId { get; set; } = null!;

        public int TableNumber { get; set; }

        public int RestaurantId { get; set; }

        [Required]
        public int Capacity { get; set; }
    }
}