using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class RolesModel : PageModel
    {
        public List<Role> Roles { get; set; } = new List<Role>();
        private string? _connectionString;
        public RolesModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }
        public void OnGet()
        {
            Roles = GetAllRoles(_connectionString);
        }

        public static List<Role> GetAllRoles(string connectionString)
        {
            List<Role> roles = new List<Role>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT RoleId, RoleName FROM [dbo].[ROLE]";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(new Role
                        {
                            RoleId = reader.GetInt32(0),
                            RoleName = reader.GetString(1)
                        });
                    }
                }
            }
            return roles;
        }
    }
}
