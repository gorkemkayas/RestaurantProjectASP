using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class OrderModel : PageModel
    {
        private readonly string? _connectionString;

        public OrderModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty(SupportsGet = true)]
        public int TableId { get; set; } // Query parametresini yakalamak için

        public List<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();

        public void OnGet()
        {
            if (TableId > 0)
            {
                OrderDetails = GetAllOrderDetailsForTable(TableId, _connectionString);
            }
        }

        public IActionResult OnPostDelete(int orderId)
        {
            if (DeleteOrderDetail(orderId))
            {
                // Baþarýyla silindikten sonra Tables sayfasýna yönlendirme
                return RedirectToPage("/Tables");
            }

            ModelState.AddModelError(string.Empty, "Failed to delete the order detail.");
            return Page();
        }

        private List<OrderDetails> GetAllOrderDetailsForTable(int tableId, string connectionString)
        {
            var orderDetails = new List<OrderDetails>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // JOIN sorgusu ile MenuItem Name'i alýyoruz
                string query = @"
                    SELECT od.OrderDetailId, od.MenuItemId, od.Quantity, od.Price, od.TableId, mi.Name 
                    FROM [dbo].[ORDERDETAILS] od
                    INNER JOIN [dbo].[MENUITEM] mi ON od.MenuItemId = mi.MenuItemId
                    WHERE od.TableId = @TableId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableId", tableId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orderDetails.Add(new OrderDetails
                            {
                                OrderDetailId = reader.GetInt32(0),
                                MenuItemId = reader.GetInt32(1),
                                Quantity = reader.GetInt32(2),
                                Price = (float)reader.GetDouble(3),
                                TableId = reader.GetInt32(4),
                                Name = reader.GetString(5)
                            });
                        }
                    }
                }
            }
            return orderDetails;
        }

        private bool DeleteOrderDetail(int orderId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "DELETE FROM dbo.ORDERDETAILS WHERE OrderDetailId = @OrderId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error occurred while deleting the order detail: {ex.Message}");
                return false;
            }
        }
    }
}
