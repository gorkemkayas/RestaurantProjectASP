using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class RoleDeleteModel : PageModel
    {
        private string? _connectionString;

        public RoleDeleteModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }
        [BindProperty]
        public int RoleId { get; set; }

        [BindProperty]
        public string RoleName { get; set; } = null!;
        public void OnGet(int id, string Name)
        {
            RoleId = id;
            RoleName = Name;
        }

        public IActionResult OnPost(int RoleId) {
            try
            {
            DeleteRoleToDatabase(RoleId);
            return RedirectToPage("/Roles");

            }
            catch (Exception)
            {
                ViewData["ConflictMessage"] = "We cannot perform this operation because <b>there are employees associated with the role</b> information you want to delete.\n <b> To perform the deletion operation, delete the employees associated with the role.</b>";
                return Page();
            }
        }
        private void DeleteRoleToDatabase(int RoleId)
        {

            string query = "DELETE FROM dbo.ROLE WHERE RoleId = @RoleId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RoleId", RoleId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
