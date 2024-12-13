using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class InventoryEditModel : PageModel
    {
        private readonly string? _connectionString;

        public InventoryEditModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        // Properties for binding form data
        [BindProperty]
        public int InventoryId { get; set; }

        [BindProperty]
        public string IngredientName { get; set; } = null!;

        [BindProperty]
        public int QuantityInStock { get; set; }

        [BindProperty]
        public string Unit { get; set; } = null!;

        // GET method to load data for the specified inventory
        public void OnGet(int id)
        {
            InventoryId = id;  // Set InventoryId for editing
            LoadInventoryData(id);  // Call method to load data from the database
        }

        // POST method to update the inventory data in the database
        public IActionResult OnPost()
        {
            try
            {
                UpdateInventoryInDatabase();  // Call method to update the database
                return RedirectToPage("/Inventory");  // Redirect back to the inventory page after successful update
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error updating inventory.");
                return Page();  // Return the page with error if the update fails
            }
        }

        // Method to load inventory data from the database
        private void LoadInventoryData(int id)
        {
            string query = "SELECT IngredientName, QuantityInStock, Unit FROM dbo.INVENTORY WHERE InventoryId = @InventoryId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InventoryId", id);  // Use the ID to get the correct record
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())  // Check if a record is found
                        {
                            IngredientName = reader["IngredientName"].ToString()!;
                            QuantityInStock = Convert.ToInt32(reader["QuantityInStock"]);
                            Unit = reader["Unit"].ToString()!;
                        }
                    }
                }
            }
        }

        // Method to update the inventory data in the database
        private void UpdateInventoryInDatabase()
        {
            string query = "UPDATE dbo.INVENTORY SET IngredientName = @IngredientName, QuantityInStock = @QuantityInStock, Unit = @Unit WHERE InventoryId = @InventoryId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IngredientName", IngredientName);  // Pass the form data to the query
                    command.Parameters.AddWithValue("@QuantityInStock", QuantityInStock);
                    command.Parameters.AddWithValue("@Unit", Unit);
                    command.Parameters.AddWithValue("@InventoryId", InventoryId);  // Pass the ID to update the correct record
                    command.ExecuteNonQuery();  // Execute the update query
                }
            }
        }
    }
}
