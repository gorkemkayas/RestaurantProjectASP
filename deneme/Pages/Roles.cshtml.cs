using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class RolesModel : PageModel
    {
        public List<Role> Roles { get; set; } = new List<Role>();
        public static readonly string connectionString = "Data Source=KAYAS;Initial Catalog=RestaurantDb;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";
        public void OnGet()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT RoleId, RoleName FROM [dbo].[ROLE]";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Roles.Add(new Role
                        {
                            RoleId = reader.GetInt32(0),
                            RoleName = reader.GetString(1)
                        });
                    }
                }
            }
        }
    }
}
