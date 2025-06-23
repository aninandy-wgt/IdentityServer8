using System.Security.Claims;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerAspNetIdentity.Pages.Roles
{
    [Authorize(Roles = AppRoles.Admin)]
    public class CreateRoleModel(RoleManager<ApplicationRole> roleManager) : PageModel
    {
        [BindProperty]
        public string? RoleName { get; set; }

        [BindProperty]
        public string? PermissionsInput { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(RoleName))
            {
                ModelState.AddModelError(nameof(RoleName), "Role name is required.");
                return Page();
            }

            var role = new ApplicationRole { Name = RoleName.Trim() };
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(PermissionsInput))
            {
                var perms = PermissionsInput.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());

                foreach (var perm in perms) await roleManager.AddClaimAsync(role, new Claim("permission", perm));
              
            }
            return RedirectToPage("ListRoles");
        }
    }
}
