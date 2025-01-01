using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class InventoryAddModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        [BindProperty]
        public string IngredientName { get; set; } = string.Empty;

        [BindProperty]
        public int QuantityInStock { get; set; }

        [BindProperty]
        public string Unit { get; set; } = string.Empty;

        public InventoryAddModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("MSSQL");
        }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(IngredientName) || QuantityInStock <= 0 || string.IsNullOrWhiteSpace(Unit))
            {
                ModelState.AddModelError("", "All fields are required and must be valid.");
                return Page();
            }

            try
            {
                AddInventoryToDatabase(IngredientName, QuantityInStock, Unit);
                return RedirectToPage("/Inventory");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return Page();
            }
        }

        private void AddInventoryToDatabase(string ingredientName, int quantityInStock, string unit)
        {
            string query = "INSERT INTO INVENTORY (IngredientName, QuantityInStock, Unit) VALUES (@IngredientName, @QuantityInStock, @Unit)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IngredientName", ingredientName);
                    command.Parameters.AddWithValue("@QuantityInStock", quantityInStock);
                    command.Parameters.AddWithValue("@Unit", unit);
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}
