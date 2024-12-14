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
                return RedirectToPage("/TableState");
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

                // Updated query to exclude orders with status "Completed"
                string query = @"
            SELECT od.OrderDetailId, od.OrderId, od.MenuItemId, od.Quantity, od.Price, od.TableId, mi.Name
            FROM [dbo].[ORDERDETAILS] od
            INNER JOIN [dbo].[MENUITEM] mi ON od.MenuItemId = mi.MenuItemId
            INNER JOIN [dbo].[ORDER] o ON od.OrderId = o.OrderId
            WHERE od.TableId = @TableId AND o.OrderStatus <> 'Completed'";  // Add condition for OrderStatus

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
                                OrderId = reader.GetInt32(1),
                                MenuItemId = reader.GetInt32(2),
                                Quantity = reader.GetInt32(3),
                                Price = (float)reader.GetDouble(4),
                                TableId = reader.GetInt32(5),
                                Name = reader.GetString(6)
                            });
                        }
                    }
                }
            }
            return orderDetails;
        }




        private void UpdateRevenue(float grandTotal, SqlConnection connection)
        {
            try
            {
                // Revenue tablosunda toplam geliri g�ncelle
                string updateRevenueQuery = @"
        UPDATE dbo.REVENUE
        SET TotalRevenue = TotalRevenue + @GrandTotal
        WHERE StartDate <= @CurrentDate AND EndDate >= @CurrentDate";

                using var updateCommand = new SqlCommand(updateRevenueQuery, connection);
                updateCommand.Parameters.AddWithValue("@GrandTotal", grandTotal);
                updateCommand.Parameters.AddWithValue("@CurrentDate", DateTime.Now);

                int rowsAffected = updateCommand.ExecuteNonQuery();

                // E�er mevcut bir gelir kayd� yoksa yeni bir kay�t ekle
                if (rowsAffected == 0)
                {
                    string insertRevenueQuery = @"
            INSERT INTO dbo.REVENUE (StartDate, EndDate, TotalRevenue, TimePeriodType)
            VALUES (@StartDate, @EndDate, @GrandTotal, @TimePeriodType)";

                    using var insertCommand = new SqlCommand(insertRevenueQuery, connection);
                    insertCommand.Parameters.AddWithValue("@StartDate", DateTime.Now.Date);
                    insertCommand.Parameters.AddWithValue("@EndDate", DateTime.Now.Date.AddDays(1).AddTicks(-1)); // G�n sonu
                    insertCommand.Parameters.AddWithValue("@GrandTotal", grandTotal);
                    insertCommand.Parameters.AddWithValue("@TimePeriodType", "Daily"); // �rne�in g�nl�k bazda

                    insertCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating revenue: {ex.Message}");
            }
        }

        public IActionResult OnPostPayment(string paymentMethod)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                // Tabloya ait t�m sipari�leri al
                string query = "SELECT OrderId, SUM(Quantity * Price) AS TotalAmount FROM dbo.ORDERDETAILS WHERE TableId = @TableId GROUP BY OrderId";
                var orders = new List<(int OrderId, float TotalAmount)>();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableId", TableId);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        orders.Add((reader.GetInt32(0), (float)reader.GetDouble(1)));
                    }
                }

                // Toplam tutar� hesapla
                var grandTotal = orders.Sum(order => order.TotalAmount);

                // Payment kayd� ekle
                string paymentQuery = @"
        INSERT INTO dbo.PAYMENT (OrderId, PaymentDate, Amount, PaymentMethod)
        VALUES (@OrderId, @PaymentDate, @Amount, @PaymentMethod)";

                foreach (var order in orders)
                {
                    using var paymentCommand = new SqlCommand(paymentQuery, connection);
                    paymentCommand.Parameters.AddWithValue("@OrderId", order.OrderId);
                    paymentCommand.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                    paymentCommand.Parameters.AddWithValue("@Amount", order.TotalAmount);
                    paymentCommand.Parameters.AddWithValue("@PaymentMethod", paymentMethod);

                    paymentCommand.ExecuteNonQuery();
                }

                // Revenue tablosunu g�ncelle
                UpdateRevenue(grandTotal, connection);

                TempData["SuccessMessage"] = $"Payment of {grandTotal:C} for Table {TableId} using {paymentMethod} has been successfully processed.";
                return RedirectToPage("/TableState");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error occurred while processing the payment: {ex.Message}");
                return Page();
            }
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
