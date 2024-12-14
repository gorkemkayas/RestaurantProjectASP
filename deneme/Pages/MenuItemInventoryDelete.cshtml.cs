using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class MenuItemInventoryDeleteModel : PageModel
    {
        private string? _connectionString;

        public MenuItemInventoryDeleteModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int MenuItemInventoryId { get; set; }

        public string MenuItemName { get; set; }
        public string InventoryName { get; set; }

        public void OnGet(int menuItemInventoryId, string menuItemName, string inventoryName)
        {
            MenuItemInventoryId = menuItemInventoryId;
            MenuItemName = menuItemName;
            InventoryName = inventoryName;
        }

        public IActionResult OnPost(int menuItemInventoryId)
        {
            try
            {
                DeleteMenuItemInventory(menuItemInventoryId);
                return RedirectToPage("/MenuItems");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error deleting the inventory item.");
                return Page();
            }
        }

        private void DeleteMenuItemInventory(int menuItemInventoryId)
        {
            string query = "DELETE FROM dbo.MENUITEM_INVENTORY WHERE MenuItemInventoryId = @MenuItemInventoryId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemInventoryId", menuItemInventoryId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
