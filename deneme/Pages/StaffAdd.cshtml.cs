using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

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
            AddStaffToDatabase(Name, RoleId,Phone);
            return RedirectToPage("/Index");
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
        private void AddStaffToDatabase(string staffName,int roleId, string staffPhone)
        {

            string query = $"INSERT INTO STAFF (Name,RoleId,Phone) VALUES (@StaffName,@StaffRoleId,@StaffPhone)";
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StaffName", staffName);
                        command.Parameters.AddWithValue("@StaffRoleId", roleId);
                        command.Parameters.AddWithValue("@StaffPhone", staffPhone);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {

                    throw new Exception("AddRole metodunda hata oluştu.");
                }
            }
        }
    }
}
