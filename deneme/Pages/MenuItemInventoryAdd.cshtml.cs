using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace deneme.Pages
{
    public class MenuItemInventoryAddModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<MenuItem> MenuItems { get; set; }
        public List<Inventory> Inventories { get; set; }
        private readonly string connectionString;

        public MenuItemInventoryAddModel(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("MSSQL");
            MenuItems = GetMenuItems();
            Inventories = GetInventories();
        }

        public void OnGet() { }

        public IActionResult OnPost(int MenuItemId, int InventoryId, int RequiredQuantity)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO [dbo].[MENUITEM_INVENTORY] (MenuItemId, InventoryId, RequiredQuantity) " +
                               "VALUES (@MenuItemId, @InventoryId, @RequiredQuantity)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MenuItemId", MenuItemId);
                command.Parameters.AddWithValue("@InventoryId", InventoryId);
                command.Parameters.AddWithValue("@RequiredQuantity", RequiredQuantity);
                command.ExecuteNonQuery();
            }

            return RedirectToPage("/MenuItemInventory"); // Redirect after adding
        }

        private List<MenuItem> GetMenuItems()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT MenuItemId, Name FROM [dbo].[MENUITEM]";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        menuItems.Add(new MenuItem
                        {
                            MenuItemId = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return menuItems;
        }

        private List<Inventory> GetInventories()
        {
            List<Inventory> inventories = new List<Inventory>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT InventoryId, IngredientName FROM [dbo].[INVENTORY]";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        inventories.Add(new Inventory
                        {
                            InventoryId = reader.GetInt32(0),
                            IngredientName = reader.GetString(1)
                        });
                    }
                }
            }
            return inventories;
        }
    }
}
