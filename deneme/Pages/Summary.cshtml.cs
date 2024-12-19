using deneme.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class SummaryModel : PageModel
    {
        private readonly string _connectionString;

        public List<float> DailyRevenues { get; private set; } = new List<float>();
        public List<float> WeeklyPercentages { get; private set; } = new List<float>();

        public SummaryModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        public void OnGet()
        {
            // Son 7 günün gelirlerini al
            var lastSevenDays = GetLastSevenDaysRevenue();

            // DailyRevenues listesini doldur
            DailyRevenues = lastSevenDays
                .OrderBy(data => data.StartDate) // Tarihe göre sıralama (istenirse)
                .Select(data => data.Revenue)
                .ToList();

            // Haftalık yüzdeleri hesaplama
            float totalWeeklyRevenue = DailyRevenues.Sum();
            if (totalWeeklyRevenue > 0)
            {
                WeeklyPercentages = DailyRevenues
                    .Select(revenue => (revenue / totalWeeklyRevenue) * 100)
                    .ToList();
            }
        }

        private List<(DateTime StartDate, float Revenue)> GetLastSevenDaysRevenue()
        {
            var revenues = new List<(DateTime StartDate, float Revenue)>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
            SELECT TOP 7 TotalRevenue, StartDate  
            FROM [dbo].[Revenue] 
            ORDER BY StartDate DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var startDate = reader.GetDateTime(reader.GetOrdinal("StartDate"));
                            var revenue = (float)reader.GetDouble(reader.GetOrdinal("TotalRevenue"));

                            revenues.Add((startDate, revenue));
                        }
                    }
                }
            }

            return revenues;
        }


        




    }
}
