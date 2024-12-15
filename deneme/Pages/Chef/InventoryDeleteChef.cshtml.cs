using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages.Chef
{
    public class InventoryDeleteChefModel : PageModel
    {
        private string? _connectionString;

        public InventoryDeleteChefModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int inventoryId { get; set; }

        [BindProperty]
        public string inventoryName { get; set; }

        public void OnGet(int id, string name)
        {
            inventoryId = id;
            inventoryName = name;
        }

        public IActionResult OnPost(int inventoryId)
        {
            try
            {
                DeleteInventoryFromDatabase(inventoryId);
                return RedirectToPage("/Chef/InventoryChef");
            }
            catch (Exception)
            {
                ViewData["Conflict"] = $"<p> The Inventory is used in somewhere. For deleting a Menu, <b>you should remove related entity.</b></p>";
                ModelState.AddModelError(string.Empty, "DeleteInventoryFromDatabase method error...");
                return Page();
            }
        }

        private void DeleteInventoryFromDatabase(int inventoryId)
        {
            string query = "DELETE FROM dbo.INVENTORY WHERE InventoryId = @InventoryId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InventoryId", inventoryId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
