using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace deneme.Pages.Waiter
{
    public class OrderAddWaiterModel : PageModel
    {
        private readonly string _connectionString;

        public OrderAddWaiterModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty(SupportsGet = true)]
        public int TableId { get; set; }

        [BindProperty]
        public List<OrderDetails> OrderDetails { get; set; } = new();

        public List<MenuItem> MenuItems { get; set; } = new();

        public void OnGet(int tableId)
        {
            TableId = tableId;
            LoadMenuItems();
        }

        public IActionResult OnPost()
        {
            if (!OrderDetails.Any(od => od.Quantity > 0))
            {
                ModelState.AddModelError(string.Empty, "Please add at least one item.");
                LoadMenuItems();
                return Page();
            }

            if (SaveOrderAndDetails())
            {
                return RedirectToPage("/Waiter/OrdersWaiter", new { TableId });
            }

            ModelState.AddModelError(string.Empty, "Failed to save the order.");
            LoadMenuItems();
            return Page();
        }

        private void LoadMenuItems()
        {
            using var connection = new SqlConnection(_connectionString);
            string query = "SELECT MenuItemId, Name, Price FROM dbo.MENUITEM";
            connection.Open();

            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                MenuItems.Add(new MenuItem
                {
                    MenuItemId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = (float)reader.GetDouble(2)
                });
            }
        }

        private bool SaveOrderAndDetails()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                // RestaurantId sabit olarak 1 atanýyor, PaymentDate ise sipariþ tarihi
                string orderQuery = @"
            INSERT INTO dbo.[ORDER] (TableId, OrderDate, OrderStatus, RestaurantId, PaymentDate)
            OUTPUT INSERTED.OrderId
            VALUES (@TableId, @OrderDate, @OrderStatus, @RestaurantId, @PaymentDate)";

                int orderId;
                using (var orderCommand = new SqlCommand(orderQuery, connection))
                {
                    orderCommand.Parameters.AddWithValue("@TableId", TableId);
                    orderCommand.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                    orderCommand.Parameters.AddWithValue("@OrderStatus", "Pending");
                    orderCommand.Parameters.AddWithValue("@RestaurantId", 1); // Sabit bir deðer olarak atanýyor
                    orderCommand.Parameters.AddWithValue("@PaymentDate", DateTime.Now); // Þu anki tarih ve saat

                    orderId = (int)orderCommand.ExecuteScalar();
                }

                string detailQuery = @"
            INSERT INTO dbo.ORDERDETAILS (OrderId, MenuItemId, Quantity, Price, TableId)
            VALUES (@OrderId, @MenuItemId, @Quantity, @Price, @TableId)";

                foreach (var detail in OrderDetails.Where(od => od.Quantity > 0))
                {
                    using var detailCommand = new SqlCommand(detailQuery, connection);
                    detailCommand.Parameters.AddWithValue("@OrderId", orderId);
                    detailCommand.Parameters.AddWithValue("@MenuItemId", detail.MenuItemId);
                    detailCommand.Parameters.AddWithValue("@Quantity", detail.Quantity);
                    detailCommand.Parameters.AddWithValue("@Price", detail.Price);
                    detailCommand.Parameters.AddWithValue("@TableId", TableId);

                    detailCommand.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return false;
            }
        }
    }
}