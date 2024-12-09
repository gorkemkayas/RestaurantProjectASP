using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace deneme.Pages
{
    public class MenuItemInventoryModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<MenuItemInventory> MenuItemInventories { get; set; } = new List<MenuItemInventory>();
        private readonly string connectionString;

        public MenuItemInventoryModel(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("MSSQL");
        }

        public void OnGet()
        {
            // Fetch all menu item inventory data from the database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT MenuItemId, InventoryId, RequiredQuantity FROM [dbo].[MENUITEM_INVENTORY]";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MenuItemInventories.Add(new MenuItemInventory
                        {
                            MenuItemId = reader.GetInt32(0),
                            InventoryId = reader.GetInt32(1),
                            RequiredQuantity = reader.GetInt32(2)
                        });
                    }
                }
            }
        }

        // Optional: OnPost method to handle adding new inventory links to menu items
        public IActionResult OnPostAdd(int menuItemId, int inventoryId, int requiredQuantity)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO [dbo].[MENUITEM_INVENTORY] (MenuItemId, InventoryId, RequiredQuantity) " +
                               "VALUES (@MenuItemId, @InventoryId, @RequiredQuantity)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MenuItemId", menuItemId);
                command.Parameters.AddWithValue("@InventoryId", inventoryId);
                command.Parameters.AddWithValue("@RequiredQuantity", requiredQuantity);

                command.ExecuteNonQuery();
            }

            return RedirectToPage(); // Refresh the page after adding new entry
        }
    }
}
