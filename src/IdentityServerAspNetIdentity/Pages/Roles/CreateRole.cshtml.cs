using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerAspNetIdentity.Pages.Roles
{
    [Authorize(Roles = "AAA_Admin")]
    public class CreateRoleModel(RoleManager<ApplicationRole> roleManager) : PageModel
    {
        [BindProperty]
        public string? RoleName { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrWhiteSpace(RoleName))
            {
                var result = await roleManager.CreateAsync(new ApplicationRole { Name = RoleName });
                if (result.Succeeded) return RedirectToPage("ListRoles");
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }
            return Page();
        }
    }
}
