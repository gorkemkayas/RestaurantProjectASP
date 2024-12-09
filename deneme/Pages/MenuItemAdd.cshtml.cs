using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace deneme.Pages
{
    public class MenuItemAddModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<Menu> Menus { get; set; } = new List<Menu>();
        private readonly string connectionString;

        public MenuItemAddModel(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("MSSQL");
        }

        public void OnGet()
        {
            Menus = GetMenus(); // Populate the dropdown with available menus
        }

        public IActionResult OnPost(int MenuId, string Name, double Price, string Description)
        {
            if (MenuId <= 0 || string.IsNullOrWhiteSpace(Name) || Price <= 0)
            {
                ModelState.AddModelError("", "Invalid input. Please provide valid Menu ID, Name, and Price.");
                Menus = GetMenus(); // Ensure menus are available on redisplay
                return Page();
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO [dbo].[MENUITEM] (MenuId, Name, Price, Description) " +
                                   "VALUES (@MenuId, @Name, @Price, @Description)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MenuId", MenuId);
                        command.Parameters.AddWithValue("@Name", Name);
                        command.Parameters.AddWithValue("@Price", Price);
                        command.Parameters.AddWithValue("@Description", Description ?? (object)DBNull.Value); // Handle null description
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToPage("/MenuItem"); // Redirect to the MenuItem page after adding
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                Menus = GetMenus(); // Ensure menus are available on redisplay
                return Page();
            }
        }

        private List<Menu> GetMenus()
        {
            List<Menu> menus = new List<Menu>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT MenuId, Name FROM [dbo].[MENU]";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                menus.Add(new Menu
                                {
                                    MenuId = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Failed to load menus: {ex.Message}");
            }
            return menus;
        }
    }
}
