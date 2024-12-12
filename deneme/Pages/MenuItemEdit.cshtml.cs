using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace deneme.Pages
{
    public class MenuItemEditModel : PageModel
    {
        private string? _connectionString;

        public MenuItemEditModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int MenuItemId { get; set; }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        [BindProperty]
        public string Description { get; set; }

        public IActionResult OnGet(int id)
        {
            // Load the menu item by ID
            var menuItem = GetMenuItemById(id);

            if (menuItem == null)
            {
                return NotFound();
            }

            MenuItemId = menuItem.MenuItemId;
            Name = menuItem.Name;
            Price = menuItem.Price;
            Description = menuItem.Description;

            return Page();
        }

        public IActionResult OnPost()
        {
            try
            {
                UpdateMenuItem(MenuItemId, Name, Price, Description);
                return RedirectToPage("/MenuItem"); // Make sure this points to the correct page
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error updating menu item.");
                return Page();
            }
        }

        private void UpdateMenuItem(int menuItemId, string name, decimal price, string description)
        {
            string query = "UPDATE dbo.MENUITEM SET Name = @Name, Price = @Price, Description = @Description WHERE MenuItemId = @MenuItemId";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemId", menuItemId);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@Description", description);

                    command.ExecuteNonQuery();
                }
            }
        }

        private dynamic GetMenuItemById(int menuItemId)
        {
            string query = "SELECT * FROM dbo.MENUITEM WHERE MenuItemId = @MenuItemId";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemId", menuItemId);
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        return new
                        {
                            MenuItemId = reader["MenuItemId"],
                            Name = reader["Name"],
                            Price = Convert.ToDecimal(reader["Price"]), // Explicit conversion to decimal
                            Description = reader["Description"]
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}
