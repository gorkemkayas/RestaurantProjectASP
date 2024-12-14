using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class LoginModel : PageModel
    {
        private string? _connectionString;

        public LoginModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int StaffId { get; set; }
        [BindProperty]
        public string Name { get; set; } = null!;
        [BindProperty]
        public int RoleId { get; set; }
        [BindProperty]
        public string Phone { get; set; } = null!;
        [BindProperty]
        public string Email { get; set; } = null!;
        [BindProperty]
        public string Password { get; set; } = null!;
        public void OnGet()
        {
            ViewData["Layout"] = "_LoginLayout";
        }
        public IActionResult OnPost()
        {

            try
            {
                GetStaffInfo(Email, Password);
                if(StaffId == 0)
                {
                    ModelState.AddModelError(string.Empty, "No such user found.");
                    return Page();
                }
                if (RoleId == 1)
                {
                    return RedirectToPage("/Index");
                }
                else if (RoleId == 2) 
                {
                    return RedirectToPage("/Waiter/IndexWaiter");
                }
                else if(RoleId == 3)
                {
                    return RedirectToPage("/Chef/IndexChef");
                }
                ModelState.AddModelError(string.Empty, "Users without the role cannot log in.");
                return Page();

            }
            catch (Exception ex)
            {

                ModelState.AddModelError(string.Empty, "Böyle bir kullanıcı bulunmamakta.");
                return Page();
            }
        }

        public void GetStaffInfo(string email, string password)
        {


            string query = $"SELECT StaffId, Name, RoleId, Phone, Email, Password FROM dbo.STAFF WHERE Email = @Email AND Password = @Password";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StaffId = reader.GetInt32(0);
                        Name = reader.GetString(1);
                        RoleId = reader.GetInt32(2);
                        Phone = reader.GetString(3);
                        Email = reader.GetString(4);
                        Password = reader.GetString(5);
                    }
                }
            }
        }
    }
}
