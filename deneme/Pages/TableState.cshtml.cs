using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace deneme.Pages
{
    public class TableStateModel : PageModel
    {
        private readonly string? _connectionString;

        // List to hold table order status
        public List<TableOrderStatus> TableStatuses { get; set; } = new List<TableOrderStatus>();

        public TableStateModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");

            if (_connectionString == null)
            {
                throw new InvalidOperationException("Connection string 'MSSQL' not found.");
            }
        }

        public void OnGet()
        {
            // Fetch table order statuses
            TableStatuses = GetOrderStatus(_connectionString);
        }

        // Method to fetch OrderStatus information from the Orders table
        public static List<TableOrderStatus> GetOrderStatus(string connectionString)
        {
            List<TableOrderStatus> tableStatuses = new List<TableOrderStatus>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // SQL query to fetch TableId and associated OrderStatus from Orders table
                string query = @"
                    SELECT t.TableId, t.TableNumber, o.OrderStatus
                    FROM [dbo].[TABLES] t
                    LEFT JOIN [dbo].[ORDER] o ON t.TableId = o.TableId
                    ORDER BY t.TableNumber";

                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    // Read through the result set
                    while (reader.Read())
                    {
                        tableStatuses.Add(new TableOrderStatus
                        {
                            TableId = reader.GetInt32(0),
                            TableNumber = reader.GetInt32(1),
                            OrderStatus = reader.IsDBNull(2) ? "No Order" : reader.GetString(2) // Handle null OrderStatus
                        });
                    }
                }
            }

            return tableStatuses;
        }
    }

    // Model to represent the table and its order status
    public class TableOrderStatus
    {
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public string OrderStatus { get; set; }
    }
}
