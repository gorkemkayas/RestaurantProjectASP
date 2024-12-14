using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace deneme.Pages
{

    
    public class IndexWaiterModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexWaiterModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        
        public void OnGet()
        {
        }
    }
}
