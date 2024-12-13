using System.ComponentModel.DataAnnotations;

namespace deneme.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int OrderId { get; set; }

        public DateTime PaymentDate { get; set; }

        public float Amount { get; set; }

        public string PaymentMethod { get; set; }
    }
}