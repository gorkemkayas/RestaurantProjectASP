using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class StaffDeleteModel : PageModel
    {
        private string? _connectionString;

        public StaffDeleteModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int staffId { get; set; }

        [BindProperty]
        public string staffName { get; set; }
        public void OnGet(int id, string Name)
        {
            staffId = id;
            staffName = Name;
        }

        public IActionResult OnPost(int staffId)
        {
            try
            {
                DeleteStaffToDatabase(staffId);
                return RedirectToPage("/Staffs");

            }
            catch (Exception)
            {

                ModelState.AddModelError(string.Empty, "DeleteStaffToDatabase method error...");
                return Page();
            }
        }

        private void DeleteStaffToDatabase(int stuffId)
        {

            string query = "DELETE FROM dbo.STAFF WHERE StaffId = @StaffId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StaffId", stuffId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
