using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace deneme.Pages
{
    public class MenuItemInventoryEditModel : PageModel
    {
        private string? _connectionString;

        public MenuItemInventoryEditModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        // Bind properties for the form fields
        [BindProperty]
        public int MenuItemId { get; set; }

        [BindProperty]
        public int InventoryId { get; set; }

        [BindProperty]
        public decimal RequiredQuantity { get; set; }

        // Properties for storing data to populate dropdowns
        public string MenuItemName { get; set; }
        public string InventoryItemName { get; set; }

        public List<MenuItem> MenuItems { get; set; }
        public List<InventoryItem> InventoryItems { get; set; }

        public IActionResult OnGet(int menuItemId, int inventoryId)
        {
            // Fetch the MenuItemInventory by MenuItemId and InventoryId
            var menuItemInventory = GetMenuItemInventoryById(menuItemId, inventoryId);

            if (menuItemInventory == null)
            {
                return NotFound();
            }

            MenuItemId = menuItemInventory.MenuItemId;
            InventoryId = menuItemInventory.InventoryId;
            RequiredQuantity = menuItemInventory.RequiredQuantity;

            // Fetch associated MenuItem and Inventory names for display
            MenuItemName = GetMenuItemNameById(MenuItemId);
            InventoryItemName = GetInventoryItemNameById(InventoryId);

            // Fetch lists for dropdown
            MenuItems = GetAllMenuItems();
            InventoryItems = GetAllInventoryItems();

            return Page();
        }

        public IActionResult OnPost()
        {
            try
            {
                // Update the MenuItemInventory with the new values
                UpdateMenuItemInventory(MenuItemId, InventoryId, RequiredQuantity);
                return RedirectToPage("/MenuItemInventory"); // Redirect back to MenuItems page
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error updating MenuItem Inventory.");
                return Page();
            }
        }

        private void UpdateMenuItemInventory(int menuItemId, int inventoryId, decimal requiredQuantity)
        {
            string query = "UPDATE dbo.MENUITEM_INVENTORY SET RequiredQuantity = @RequiredQuantity WHERE MenuItemId = @MenuItemId AND InventoryId = @InventoryId";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemId", menuItemId);
                    command.Parameters.AddWithValue("@InventoryId", inventoryId);
                    command.Parameters.AddWithValue("@RequiredQuantity", requiredQuantity);
                    command.ExecuteNonQuery();
                }
            }
        }

        private dynamic GetMenuItemInventoryById(int menuItemId, int inventoryId)
        {
            string query = "SELECT MenuItemId, InventoryId, RequiredQuantity FROM dbo.MENUITEM_INVENTORY WHERE MenuItemId = @MenuItemId AND InventoryId = @InventoryId";

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
                                InventoryId = reader["InventoryId"],
                                RequiredQuantity = Convert.ToDecimal(reader["RequiredQuantity"])
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

        private List<MenuItem> GetAllMenuItems()
        {
            List<MenuItem> items = new List<MenuItem>();
            string query = "SELECT MenuItemId, Name FROM dbo.MENUITEM";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        items.Add(new MenuItem
                        {
                            MenuItemId = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return items;
        }

        private List<InventoryItem> GetAllInventoryItems()
        {
            List<InventoryItem> items = new List<InventoryItem>();
            string query = "SELECT InventoryId, IngredientName FROM dbo.INVENTORY";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        items.Add(new InventoryItem
                        {
                            InventoryId = reader.GetInt32(0),
                            IngredientName = reader.GetString(1)
                        });
                    }
                }
            }
            return items;
        }
    }

    public class MenuItem
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
    }

    public class InventoryItem
    {
        public int InventoryId { get; set; }
        public string IngredientName { get; set; }
    }
}
