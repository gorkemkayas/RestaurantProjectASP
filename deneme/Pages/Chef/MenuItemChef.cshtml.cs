using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace deneme.Pages.Chef
{
    public class MenuItemChefModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        private readonly string connectionString;

        public MenuItemChefModel(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("MSSQL");
        }

        public void OnGet()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT MenuItemId, Name, Description, Price FROM [dbo].[MENUITEM]";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MenuItems.Add(new MenuItem
                        {
                            MenuItemId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            Price = reader.GetDouble(3)
                        });
                    }
                }
            }
        }
    }
}
