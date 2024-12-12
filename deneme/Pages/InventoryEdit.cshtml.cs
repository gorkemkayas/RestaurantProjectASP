using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class InventoryEditModel : PageModel
    {
        private string? _connectionString;

        public InventoryEditModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int InventoryId { get; set; }

        [BindProperty]
        public string IngredientName { get; set; } = null!;

        [BindProperty]
        public int QuantityInStock { get; set; }

        [BindProperty]
        public string Unit { get; set; } = null!;

        public void OnGet(int id)
        {
            LoadInventoryData(id);
        }

        public IActionResult OnPost()
        {
            try
            {
                UpdateInventoryInDatabase();
                return RedirectToPage("/Inventory");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error updating inventory.");
                return Page();
            }
        }

        private void LoadInventoryData(int id)
        {
            string query = "SELECT IngredientName, QuantityInStock, Unit FROM dbo.INVENTORY WHERE InventoryId = @InventoryId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InventoryId", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            IngredientName = reader["IngredientName"].ToString()!;
                            QuantityInStock = Convert.ToInt32(reader["QuantityInStock"]);
                            Unit = reader["Unit"].ToString()!;
                        }
                    }
                }
            }
        }

        private void UpdateInventoryInDatabase()
        {
            string query = "UPDATE dbo.INVENTORY SET IngredientName = @IngredientName, QuantityInStock = @QuantityInStock, Unit = @Unit WHERE InventoryId = @InventoryId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IngredientName", IngredientName);
                    command.Parameters.AddWithValue("@QuantityInStock", QuantityInStock);
                    command.Parameters.AddWithValue("@Unit", Unit);
                    command.Parameters.AddWithValue("@InventoryId", InventoryId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
