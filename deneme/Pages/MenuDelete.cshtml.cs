using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class MenuDeleteModel : PageModel
    {
        private string? _connectionString;

        public MenuDeleteModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int menuId { get; set; }

        [BindProperty]
        public string menuName { get; set; }

        public void OnGet(int id, string name)
        {
            menuId = id;
            menuName = name;
        }

        public IActionResult OnPost(int menuId)
        {
            try
            {
                DeleteMenuFromDatabase(menuId);
                return RedirectToPage("/Menu");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "DeleteMenuFromDatabase method error...");
                return Page();
            }
        }

        private void DeleteMenuFromDatabase(int menuId)
        {
            string query = "DELETE FROM dbo.MENU WHERE MenuId = @MenuId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuId", menuId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
