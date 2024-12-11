using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace deneme.Pages
{
    public class StaffEditModel : PageModel
    {
        private string? _connectionString;

        public StaffEditModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int StaffId { get; set; }

        [Required]
        [BindProperty]
        public string Name { get; set; } = null!;

        [Required]
        [BindProperty]
        public int RoleId { get; set; }

        [Required]
        [BindProperty]
        public string Phone { get; set; } = null!;

        [BindProperty]
        public RoleDropdownViewModel RoleDropdown { get; set; }

        public Staff StaffInfo;
        public void OnGet(int id)
        {
            StaffInfo = GetStaffInfo(id);
            
            StaffId = StaffInfo.StaffId;
            Name = StaffInfo.Name;
            RoleId = StaffInfo.RoleId;
            Phone = StaffInfo.Phone;

            RoleDropdown = new RoleDropdownViewModel
            {
                Roles = GetAllRoles(_connectionString).Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName
                }).ToList()
            };

            RoleDropdown.SelectedRoleId = RoleId;
        }

        public IActionResult OnPost()
        {
            try
            {
            UpdateStaffToDatabase(StaffId, Name, RoleId, Phone);
            return RedirectToPage("/Staffs");
            }
            catch (Exception)
            {

                ModelState.AddModelError(string.Empty, "UpdateStaffToDatabase Method Error...");
                return Page();
            }

        }

        public Staff GetStaffInfo(int id)
        {
            Staff staffInfo = new Staff();

            string query = $"SELECT StaffId, Name, RoleId, Phone FROM dbo.STAFF WHERE StaffId = @StaffId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@StaffId", id);


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        staffInfo.StaffId = reader.GetInt32(0);
                        staffInfo.Name = reader.GetString(1);
                        staffInfo.RoleId = reader.GetInt32(2);
                        staffInfo.Phone = reader.GetString(3);
                    }
                }
            }
            return staffInfo;
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

        private void UpdateStaffToDatabase(int staffId, string name, int roleId, string phone)
        {

            string query = $"UPDATE dbo.STAFF SET Name = @Name, RoleId = @RoleId, Phone = @Phone WHERE StaffId = @StaffId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StaffId", staffId);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@RoleId", roleId);
                    command.Parameters.AddWithValue("@Phone", phone);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
