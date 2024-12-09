using Microsoft.AspNetCore.Mvc.Rendering;

namespace deneme.Models
{
    public class RoleDropdownViewModel
    {
        public List<SelectListItem>? Roles { get; set; }
        public int SelectedRoleId { get; set; }
    }
}
