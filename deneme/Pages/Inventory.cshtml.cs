using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace deneme.Pages
{
    public class InventoryModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<Inventory> Inventories { get; set; } = new List<Inventory>();
        private readonly string connectionString;

        public InventoryModel(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("MSSQL");
        }

        public void OnGet()
        {
            // Fetch all inventory data from the database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT InventoryId, IngredientName, QuantityInStock, Unit FROM [dbo].[INVENTORY]";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Inventories.Add(new Inventory
                        {
                            InventoryId = reader.GetInt32(0),
                            IngredientName = reader.GetString(1),
                            QuantityInStock = reader.GetInt32(2),
                            Unit = reader.GetString(3)
                        });
                    }
                }
            }
        }
    }
}
