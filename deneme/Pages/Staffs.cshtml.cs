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

                string query = "SELECT StaffId, Name, s.RoleId, RoleName, Phone, Email FROM [dbo].[STAFF] s INNER JOIN [dbo].[Role] r ON s.RoleId = r.RoleId ORDER BY RoleName";
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
                            RoleName = reader.GetString(3),
                            Phone = reader.GetString(4),
                            Email = reader.GetString(5),
                        });
                    }
                }
            }
            return staffs;
        }
    }
}
