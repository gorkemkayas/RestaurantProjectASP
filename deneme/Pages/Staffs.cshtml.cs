using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace deneme.Pages
{
    public class StaffsModel : PageModel
    {
        private string? _connectionString;

        public List<Staff> Staffs { get; set; } = new List<Staff>();
        public StaffsModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }
        public void OnGet()
        {
        }
    }
}
