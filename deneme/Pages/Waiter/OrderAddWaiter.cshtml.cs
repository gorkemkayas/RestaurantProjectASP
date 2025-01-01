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

        [BindProperty]
        public int StaffId { get; set; } // Seçilen Staff ID

        public List<MenuItem> MenuItems { get; set; } = new();
        public List<Staff> StaffList { get; set; } = new(); // Staff listesi

        [BindProperty]
        public string MenuName { get; set; }

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
                try
                {
                    ChangeTableStatus(TableId, "F");
                    return RedirectToPage("/Waiter/OrdersWaiter", new { TableId });
                }
                catch (Exception ex)
                {

                    throw;
                }

            }

            ModelState.AddModelError(string.Empty, "Failed to save the order.");
            LoadMenuItems();
            LoadStaffList(); // Hatalý durumlarda tekrar staff listesini yükle
            return Page();
        }

        public void ChangeTableStatus(int tableId, string status)
        {
            string query = "UPDATE dbo.TABLES SET Status = @Status WHERE TableId = @TableId";

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableId", tableId);
                    command.Parameters.AddWithValue("@Status", status);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void LoadMenuItems()
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"
        SELECT MI.MenuItemId, MI.Name AS MenuItemName, MI.Price, MI.MenuId, M.Name AS MenuName
        FROM dbo.MENUITEM MI
        JOIN dbo.MENU M ON MI.MenuId = M.MenuId";

            connection.Open();

            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                MenuItems.Add(new MenuItem
                {
                    MenuItemId = reader.GetInt32(0),
                    Name = reader.GetString(1), // MenuItem.Name
                    Price = (float)reader.GetDouble(2),
                    MenuId = reader.GetInt32(3),
                    MenuName = reader.GetString(4) // Menu.Name
                });
            }
        }


        private void LoadStaffList()
        {
            using var connection = new SqlConnection(_connectionString);
            string query = "SELECT StaffId, Name, RoleId, Phone FROM dbo.STAFF WHERE RoleId = 2";
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
