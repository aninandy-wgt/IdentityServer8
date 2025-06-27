using System.Security.Claims;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerAspNetIdentity.Pages.Roles
{
    [Authorize(Roles = AppRoles.Admin)]
    public class EditRoleModel(RoleManager<ApplicationRole> roleManager) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string RoleName { get; set; } = "";

        public List<string> Permissions { get; set; } = [];

        [BindProperty]
        public string NewPermissions { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            var role = await roleManager.FindByNameAsync(RoleName);
            if (role == null) return NotFound();

            var claims = await roleManager.GetClaimsAsync(role);
            Permissions = [.. claims.Where(c => c.Type == "permission").Select(c => c.Value)];
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? removePermission)
        {
            var role = await roleManager.FindByNameAsync(RoleName);
            if (role == null) return NotFound();

            if (!string.IsNullOrEmpty(removePermission))
            {
                var claim = new Claim("permission", removePermission);
                await roleManager.RemoveClaimAsync(role, claim);
                return RedirectToPage(new { roleName = RoleName });
            }

            if (!string.IsNullOrWhiteSpace(NewPermissions))
            {
                var perms = NewPermissions.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());

                foreach (var perm in perms) await roleManager.AddClaimAsync(role, new Claim("permission", perm));

            }
            return RedirectToPage("ListRoles");
        }
    }
}
