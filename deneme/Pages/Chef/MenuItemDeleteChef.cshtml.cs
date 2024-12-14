using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages.Chef
{
    public class MenuItemDeleteChefModel : PageModel
    {
        private string? _connectionString;

        public MenuItemDeleteChefModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int menuItemId { get; set; }

        [BindProperty]
        public string menuItemName { get; set; }

        public void OnGet(int id, string name)
        {
            menuItemId = id;
            menuItemName = name;
        }

        public IActionResult OnPost(int menuItemId)
        {
            try
            {
                DeleteMenuItemFromDatabase(menuItemId);
                return RedirectToPage("/Chef/MenuItemChef");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "DeleteMenuItemFromDatabase method error...");
                return Page();
            }
        }

        private void DeleteMenuItemFromDatabase(int menuItemId)
        {
            string query = "DELETE FROM dbo.MENUITEM WHERE MenuItemId = @MenuItemId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemId", menuItemId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
