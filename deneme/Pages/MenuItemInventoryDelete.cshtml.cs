using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

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
        public int MenuItemId { get; set; }

        [BindProperty]
        public int InventoryId { get; set; }

        public string MenuItemName { get; set; }
        public string InventoryItemName { get; set; }

        public IActionResult OnGet(int menuItemId, int inventoryId)
        {
            var menuItemInventory = GetMenuItemInventoryById(menuItemId, inventoryId);

            if (menuItemInventory == null)
            {
                return NotFound();
            }

            MenuItemId = menuItemInventory.MenuItemId;
            InventoryId = menuItemInventory.InventoryId;

            MenuItemName = GetMenuItemNameById(MenuItemId);
            InventoryItemName = GetInventoryItemNameById(InventoryId);

            return Page();
        }

        public IActionResult OnPost()
        {
            try
            {
                DeleteMenuItemInventory(MenuItemId, InventoryId);
                return RedirectToPage("/MenuItemInventory"); // Redirect to the main page after deletion
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error deleting MenuItem Inventory.");
                return Page();
            }
        }

        private void DeleteMenuItemInventory(int menuItemId, int inventoryId)
        {
            string query = "DELETE FROM dbo.MENUITEM_INVENTORY WHERE MenuItemId = @MenuItemId AND InventoryId = @InventoryId";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemId", menuItemId);
                    command.Parameters.AddWithValue("@InventoryId", inventoryId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private dynamic GetMenuItemInventoryById(int menuItemId, int inventoryId)
        {
            string query = "SELECT MenuItemId, InventoryId FROM dbo.MENUITEM_INVENTORY WHERE MenuItemId = @MenuItemId AND InventoryId = @InventoryId";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemId", menuItemId);
                    command.Parameters.AddWithValue("@InventoryId", inventoryId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new
                            {
                                MenuItemId = reader["MenuItemId"],
                                InventoryId = reader["InventoryId"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        private string GetMenuItemNameById(int menuItemId)
        {
            string query = "SELECT Name FROM dbo.MENUITEM WHERE MenuItemId = @MenuItemId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemId", menuItemId);
                    var result = command.ExecuteScalar();
                    return result?.ToString() ?? string.Empty;
                }
            }
        }

        private string GetInventoryItemNameById(int inventoryId)
        {
            string query = "SELECT IngredientName FROM dbo.INVENTORY WHERE InventoryId = @InventoryId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InventoryId", inventoryId);
                    var result = command.ExecuteScalar();
                    return result?.ToString() ?? string.Empty;
                }
            }
        }
    }
}
