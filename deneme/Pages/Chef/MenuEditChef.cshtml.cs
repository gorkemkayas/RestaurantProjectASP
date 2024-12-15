using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages.Chef
{
    public class MenuEditChefModel : PageModel
    {
        private string? _connectionString;

        public MenuEditChefModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int MenuId { get; set; }

        [BindProperty]
        public int RestaurantId { get; set; }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Description { get; set; }

        public void OnGet(int id)
        {
            string query = "SELECT MenuId, RestaurantId, Name, Description FROM MENU WHERE MenuId = @MenuId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuId", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            MenuId = (int)reader["MenuId"];
                            RestaurantId = (int)reader["RestaurantId"];
                            Name = reader["Name"].ToString();
                            Description = reader["Description"].ToString();
                        }
                    }
                }
            }
        }

        public IActionResult OnPost()
        {
            try
            {
                UpdateMenuInDatabase();
                return RedirectToPage("/Chef/MenuChef");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }

        private void UpdateMenuInDatabase()
        {
            string query = "UPDATE MENU SET RestaurantId = @RestaurantId, Name = @Name, Description = @Description WHERE MenuId = @MenuId";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuId", MenuId);
                    command.Parameters.AddWithValue("@RestaurantId", RestaurantId);
                    command.Parameters.AddWithValue("@Name", Name);
                    command.Parameters.AddWithValue("@Description", Description);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
