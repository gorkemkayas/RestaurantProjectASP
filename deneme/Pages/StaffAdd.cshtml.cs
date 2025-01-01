using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace deneme.Pages
{
    public class StaffAddModel : PageModel
    {
        private string? _connectionString;
        public StaffAddModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

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

        public List<Role> AllRoles { get; set; } = new List<Role>();

        [BindProperty]
        public RoleDropdownViewModel RoleDropdown { get; set; }
        public void OnGet()
        {
            RoleDropdown = new RoleDropdownViewModel
            {
                Roles = GetAllRoles(_connectionString).Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName
                }).ToList()
            };

        }

        public IActionResult OnPost()
        {
            AddStaffToDatabase(Name, RoleId,Phone,Email,Password);
            return RedirectToPage("/Staffs");
        }

        public static List<Role> GetAllRoles(string connectionString)
        {
            List<Role> roles = new List<Role>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT RoleId, RoleName FROM [dbo].[ROLE]";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(new Role
                        {
                            RoleId = reader.GetInt32(0),
                            RoleName = reader.GetString(1)
                        });
                    }
                }
            }
            return roles;
        }
        private void AddStaffToDatabase(string staffName,int roleId, string staffPhone, string email, string password)
        {

            string query = $"INSERT INTO STAFF (Name,RoleId,Phone,Email,Password) VALUES (@StaffName,@StaffRoleId,@StaffPhone,@StaffEmail,@StaffPassword)";
            using (var connection = new SqlConnection(_connectionString))
            {
                string hashedPassword = ComputeSha256Hash(password);
                try
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StaffName", staffName);
                        command.Parameters.AddWithValue("@StaffRoleId", roleId);
                        command.Parameters.AddWithValue("@StaffPhone", staffPhone);
                        command.Parameters.AddWithValue("@StaffEmail", email);
                        command.Parameters.AddWithValue("@StaffPassword", hashedPassword);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {

                    throw new Exception("AddRole metodunda hata oluştu.");
                }
            }
        }

        static string ComputeSha256Hash(string input)
        {
            // SHA-256 nesnesini oluştur
            using (SHA256 sha256 = SHA256.Create())
            {
                // Giriş metnini byte dizisine dönüştür
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                // SHA-256 hash hesapla
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Hash'i string formatına dönüştür (hexadecimal)
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // Hexadecimal format
                }

                return sb.ToString();
            }
        }
    }
}
