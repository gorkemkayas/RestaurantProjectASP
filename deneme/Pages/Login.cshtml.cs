using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace deneme.Pages
{
    public class LoginModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Layout"] = "_LoginLayout";
        }
    }
}
