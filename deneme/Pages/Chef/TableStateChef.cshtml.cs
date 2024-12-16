using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages.Chef
{
    public class TableStateChefModel : PageModel
    {
        private readonly string? _connectionString;

        // List to hold table order status

        public TableStateChefModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");

            if (_connectionString == null)
            {
                throw new InvalidOperationException("Connection string 'MSSQL' not found.");
            }
        }

        [BindProperty]
        public Dictionary<int, string> TableStatuses { get; set; } = new Dictionary<int, string>();

        [Obsolete]
        public void GetStatusTable(int tableId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"SELECT Status FROM dbo.TABLES WHERE TableId = @TableId";
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TableId", tableId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string status = reader.GetString(0);
                        TableStatuses[tableId] = status;
                    }
                }
            }
        }

        public void OnGet()
        {
            for (int i = 1; i < 41; i++)
            {
                GetStatusTable(i);
            }
            // Fetch table order statuses
            //TableStatuses = GetOrderStatus(_connectionString);
            //GetStatusTable(1);
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
}
