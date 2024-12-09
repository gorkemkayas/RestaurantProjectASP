using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace deneme.Pages
{
    public class MenuModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<Menu> Menus { get; set; } = new List<Menu>();
        private readonly string connectionString;

        public MenuModel(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("MSSQL");
        }

        public void OnGet()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT MenuId, Name, Description FROM [dbo].[MENU]";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Menus.Add(new Menu
                        {
                            MenuId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2)
                        });
                    }
                }
            }
        }
    }
}
