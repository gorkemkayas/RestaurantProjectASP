using System.ComponentModel.DataAnnotations;

namespace deneme.Models
{
    public class Revenue
    {
        public int RevenueId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public float TotalRevenue { get; set; }

        public string TimePeriodType { get; set; }

    }
}
