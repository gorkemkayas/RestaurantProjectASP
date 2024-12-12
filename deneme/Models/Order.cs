using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deneme.Models
{
    public class AddOrder
    {
        public int? TableId { get; set; } = null!;

        public int OrderId { get; set; }


        public DateTime? OrderDate { get; set; } = null!;

        
        public int? CustomerId { get; set; }


        public int? RestaurantId { get; set; } = null!;


        public DateTime? PaymentDate { get; set; } = null!;

        [StringLength(50)]
        public string OrderStatus { get; set; }
    }
}
