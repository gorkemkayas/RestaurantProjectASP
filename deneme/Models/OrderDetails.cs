using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deneme.Models
{
    public class OrderDetails
    {
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }

        public int MenuItemId { get; set; }

        public int Quantity { get; set; }

        public float Price { get; set; }

        public int TableId { get; set; }

        public string Name { get; set; }
    }
}
