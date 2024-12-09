using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class RoleEditModel : PageModel
    {
        private string? _connectionString;

        public RoleEditModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }
        [BindProperty]
        public int RoleId { get; set; }

        [BindProperty]
        public string RoleName { get; set; } = null!;
        public void OnGet(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"SELECT RoleId, RoleName FROM dbo.ROLE WHERE RoleId = @RoleId";
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoleId", id);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        RoleId = reader.GetInt32(0);
                        RoleName = reader.GetString(1);
                    }
                }
            }
        }

        public IActionResult OnPost(int RoleId,string RoleName) {
            if (!ModelState.IsValid)
            {
                return Page(); 
            }
            SaveRoleToDatabase(RoleId, RoleName);
            return RedirectToPage("/Roles");
        }
        private void SaveRoleToDatabase(int id, string roleName)
        {
            
            string query = $"UPDATE dbo.ROLE SET RoleName = @RoleName WHERE RoleId = @RoleId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RoleName", roleName);
                    command.Parameters.AddWithValue("@RoleId", id);
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}
