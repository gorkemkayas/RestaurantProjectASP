using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace deneme.Pages
{
    public class MenuItemModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        private readonly string connectionString;

        public MenuItemModel(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("MSSQL");
        }

        public void OnGet()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
            SELECT MI.MenuItemId, MI.Name AS MenuItemName, MI.Description, MI.Price, M.Name AS MenuName
            FROM dbo.MENUITEM MI
            JOIN dbo.MENU M ON MI.MenuId = M.MenuId";

                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MenuItems.Add(new MenuItem
                        {
                            MenuItemId = reader.GetInt32(0),
                            Name = reader.GetString(1),         // MenuItem Name
                            Description = reader.GetString(2), // Description
                            Price = reader.GetDouble(3),       // Price
                            MenuName = reader.GetString(4)     // Menu Name from MENU table
                        });
                    }
                }
            }
        }

    }
}
