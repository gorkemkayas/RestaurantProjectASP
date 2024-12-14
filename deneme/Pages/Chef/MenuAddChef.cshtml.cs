using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace deneme.Pages.Chef
{
    public class MenuAddChefModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString;

        public MenuAddChefModel(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("MSSQL");
        }

        public void OnGet() { }

        public IActionResult OnPost(string Name, string? Description)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("", "Menu Name is required.");
                return Page();
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO [dbo].[MENU] (Name, Description) VALUES (@Name, @Description)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", Name);

                        // If Description is null or empty, use DBNull.Value
                        if (string.IsNullOrWhiteSpace(Description))
                        {
                            command.Parameters.AddWithValue("@Description", DBNull.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@Description", Description);
                        }

                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToPage("/Chef/MenuChef"); // Redirect back to the Menu page after adding
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", $"Database error: {ex.Message}");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                return Page();
            }
        }
    }
}
