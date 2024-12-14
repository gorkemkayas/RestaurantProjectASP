using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace deneme.Pages.Chef
{

    
    public class IndexChefModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexChefModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        
        public void OnGet()
        {
        }
    }
}
