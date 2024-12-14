using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages.Waiter
{
    public class OrdersWaiterModel : PageModel
    {
        private readonly string? _connectionString;

        public OrdersWaiterModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty(SupportsGet = true)]
        public int TableId { get; set; } // Query parametresini yakalamak i�in

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
                // Ba�ar�yla silindikten sonra Tables sayfas�na y�nlendirme
                return RedirectToPage("/Waiter/TableStateWaiter");
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

                // JOIN sorgusu ile MenuItem Name'i ve OrderId'yi al�yoruz
                string query = @"
            SELECT od.OrderDetailId, od.OrderId, od.MenuItemId, od.Quantity, od.Price, od.TableId, mi.Name 
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
                                OrderId = reader.GetInt32(1),  // OrderId'yi ekliyoruz
                                MenuItemId = reader.GetInt32(2),
                                Quantity = reader.GetInt32(3),
                                Price = (float)reader.GetDouble(4), // Burada Price'� al�yoruz
                                TableId = reader.GetInt32(5),
                                Name = reader.GetString(6)
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

                    // Transaction kullanarak ORDERDETAILS ve ORDER tablosundaki silme i�lemini g�vence alt�na al�yoruz.
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. ORDERDETAILS tablosundaki kayd� sil
                            string deleteOrderDetailsQuery = "DELETE FROM dbo.ORDERDETAILS WHERE OrderId = @OrderId";
                            using (var deleteOrderDetailsCommand = new SqlCommand(deleteOrderDetailsQuery, connection, transaction))
                            {
                                deleteOrderDetailsCommand.Parameters.AddWithValue("@OrderId", orderId);
                                deleteOrderDetailsCommand.ExecuteNonQuery();
                            }

                            // 2. ORDERDETAILS tablosunda, o OrderId'ye ba�l� ba�ka kay�t olup olmad���n� kontrol et
                            string checkOrderDetailsQuery = "SELECT COUNT(*) FROM dbo.ORDERDETAILS WHERE OrderId = @OrderId";
                            using (var checkOrderDetailsCommand = new SqlCommand(checkOrderDetailsQuery, connection, transaction))
                            {
                                checkOrderDetailsCommand.Parameters.AddWithValue("@OrderId", orderId);
                                int count = (int)checkOrderDetailsCommand.ExecuteScalar();

                                // E�er ba�ka OrderDetail kayd� yoksa, ORDER kayd�n� da sil
                                if (count == 0)
                                {
                                    string deleteOrderQuery = "DELETE FROM dbo.[ORDER] WHERE OrderId = @OrderId";
                                    using (var deleteOrderCommand = new SqlCommand(deleteOrderQuery, connection, transaction))
                                    {
                                        deleteOrderCommand.Parameters.AddWithValue("@OrderId", orderId);
                                        deleteOrderCommand.ExecuteNonQuery();
                                    }
                                }
                            }

                            // ��lemleri ba�ar�l� bir �ekilde tamamla
                            transaction.Commit();
                            return true;
                        }
                        catch (Exception)
                        {
                            // Hata durumunda i�lemi geri al
                            transaction.Rollback();
                            throw;
                        }
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
