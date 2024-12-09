using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace deneme.Pages
{
    public class RoleAddModel : PageModel
    {
        private string? _connectionString;

        [BindProperty]
        public string RoleName { get; set; } = null!;
        public RoleAddModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }
        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if(ModelState.IsValid)
            {
                AddRoleToDatabase(RoleName);
                return RedirectToPage("/Roles");
            }
            else
            {
                return Page();
            }
        }

        private void AddRoleToDatabase(string roleName)
        {

            string query = $"INSERT INTO ROLE (RoleName) VALUES (@RoleName)";
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoleName", roleName);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {

                    throw new Exception("AddRole metodunda hata oluştu.");
                }
            }
        }
    }
}
