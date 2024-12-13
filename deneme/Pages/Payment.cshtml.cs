using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace deneme.Pages
{
    public class PaymentModel : PageModel
    {
        private readonly string? _connectionString;

        public PaymentModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int TableId { get; set; }

        public async Task<IActionResult> OnPostAsync(int tableId)
        {
            if (_connectionString == null)
            {
                return BadRequest("Database connection string is not configured.");
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    // Step 1: Retrieve the latest order for the given TableId
                    string getOrderQuery = "SELECT TOP 1 OrderId, Quantity, Price FROM dbo.Orders WHERE TableId = @TableId ORDER BY OrderDate DESC";
                    SqlCommand getOrderCommand = new SqlCommand(getOrderQuery, connection);
                    getOrderCommand.Parameters.AddWithValue("@TableId", tableId);

                    int orderId;
                    int quantity;
                    float price;

                    using (SqlDataReader reader = await getOrderCommand.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return NotFound("No orders found for the specified table.");
                        }

                        await reader.ReadAsync();
                        orderId = reader.GetInt32(0);
                        quantity = reader.GetInt32(1);
                        price = reader.GetFloat(2);
                    }

                    // Step 2: Calculate amount and prepare payment details
                    float amount = quantity * price;
                    DateTime paymentDate = DateTime.Now;
                    string paymentMethod = "Cash";

                    // Step 3: Insert into dbo.Payment
                    string insertPaymentQuery = "INSERT INTO dbo.Payment (OrderId, PaymentDate, Amount, PaymentMethod) VALUES (@OrderId, @PaymentDate, @Amount, @PaymentMethod)";
                    SqlCommand insertPaymentCommand = new SqlCommand(insertPaymentQuery, connection);
                    insertPaymentCommand.Parameters.AddWithValue("@OrderId", orderId);
                    insertPaymentCommand.Parameters.AddWithValue("@PaymentDate", paymentDate);
                    insertPaymentCommand.Parameters.AddWithValue("@Amount", amount);
                    insertPaymentCommand.Parameters.AddWithValue("@PaymentMethod", paymentMethod);

                    await insertPaymentCommand.ExecuteNonQueryAsync();

                    return RedirectToPage("Payment", new { tableId = tableId });
                }
                catch (Exception ex)
                {
                    // Log exception and handle errors
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }
    }
}
