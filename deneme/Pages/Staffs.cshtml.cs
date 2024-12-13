using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class StaffsModel : PageModel
    {
        private string? _connectionString;

        public List<Staff> Staffs { get; set; } = new List<Staff>();
        public StaffsModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }
        public void OnGet()
        {
            Staffs = GetAllStaffs(_connectionString);
        }

        public static List<Staff> GetAllStaffs(string connectionString)
        {
            List<Staff> staffs = new List<Staff>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT StaffId, Name, RoleId, Phone FROM [dbo].[STAFF]";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        staffs.Add(new Staff
                        {
                            StaffId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            RoleId = reader.GetInt32(2),
                            Phone = reader.GetString(1)
                        });
                    }
                }
            }
            return staffs;
        }
    }
}
