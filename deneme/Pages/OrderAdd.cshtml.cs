using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace deneme.Pages
{
    public class OrderAddModel : PageModel
    {
        private readonly string _connectionString;

        public OrderAddModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty(SupportsGet = true)]
        public int TableId { get; set; }

        [BindProperty]
        public List<OrderDetails> OrderDetails { get; set; } = new();

        [BindProperty]
        public int StaffId { get; set; } // Seçilen Staff ID

        public List<MenuItem> MenuItems { get; set; } = new();
        public List<Staff> StaffList { get; set; } = new(); // Staff listesi

        public void OnGet(int tableId)
        {
            TableId = tableId;
            LoadMenuItems();
            LoadStaffList(); // Staff listesini yükle
        }

        public IActionResult OnPost()
        {
            if (!OrderDetails.Any(od => od.Quantity > 0))
            {
                ModelState.AddModelError(string.Empty, "Please add at least one item.");
                LoadMenuItems();
                LoadStaffList(); // Hatalý durumlarda tekrar staff listesini yükle
                return Page();
            }

            if (SaveOrderAndDetails())
            {
                return RedirectToPage("/Orders", new { TableId });
            }

            ModelState.AddModelError(string.Empty, "Failed to save the order.");
            LoadMenuItems();
            LoadStaffList(); // Hatalý durumlarda tekrar staff listesini yükle
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

        private void LoadStaffList()
        {
            using var connection = new SqlConnection(_connectionString);
            string query = "SELECT StaffId, Name, RoleId, Phone FROM dbo.STAFF";
            connection.Open();

            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                StaffList.Add(new Staff
                {
                    StaffId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    RoleId = reader.GetInt32(2),
                    Phone = reader.GetString(3)
                });
            }
        }

        private bool SaveOrderAndDetails()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                foreach (var detail in OrderDetails.Where(od => od.Quantity > 0))
                {
                    // Her bir menü öðesi için ayrý bir sipariþ (Order) oluþtur
                    string orderQuery = @"
                INSERT INTO dbo.[ORDER] (TableId, OrderDate, OrderStatus, RestaurantId, PaymentDate, StaffId)
                OUTPUT INSERTED.OrderId
                VALUES (@TableId, @OrderDate, @OrderStatus, @RestaurantId, @PaymentDate, @StaffId)";

                    int orderId;
                    using (var orderCommand = new SqlCommand(orderQuery, connection))
                    {
                        orderCommand.Parameters.AddWithValue("@TableId", TableId);
                        orderCommand.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                        orderCommand.Parameters.AddWithValue("@OrderStatus", "Pending");
                        orderCommand.Parameters.AddWithValue("@RestaurantId", 1); // Sabit bir deðer olarak atanýyor
                        orderCommand.Parameters.AddWithValue("@PaymentDate", DateTime.Now); // Þu anki tarih ve saat
                        orderCommand.Parameters.AddWithValue("@StaffId", StaffId); // Seçilen StaffId

                        orderId = (int)orderCommand.ExecuteScalar();
                    }

                    // Sipariþ detaylarýný ekle
                    string detailQuery = @"
                INSERT INTO dbo.ORDERDETAILS (OrderId, MenuItemId, Quantity, Price, TableId)
                VALUES (@OrderId, @MenuItemId, @Quantity, @Price, @TableId)";

                    using var detailCommand = new SqlCommand(detailQuery, connection);
                    detailCommand.Parameters.AddWithValue("@OrderId", orderId);
                    detailCommand.Parameters.AddWithValue("@MenuItemId", detail.MenuItemId);
                    detailCommand.Parameters.AddWithValue("@Quantity", detail.Quantity);
                    detailCommand.Parameters.AddWithValue("@Price", detail.Price);
                    detailCommand.Parameters.AddWithValue("@TableId", TableId);
                    detailCommand.Parameters.AddWithValue("@StaffId", detail.StaffId);

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
