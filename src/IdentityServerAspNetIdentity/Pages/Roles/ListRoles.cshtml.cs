using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerAspNetIdentity.Pages.Roles
{
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Viewer},{AppRoles.ProjectManager}")]
    public class ListRolesModel(RoleManager<ApplicationRole> roleManager) : PageModel
    {
        public List<RoleWithClaims> RolesWithClaims { get; set; } = [];

        public async Task OnGetAsync()
        {
            var roles = roleManager.Roles.ToList();
            foreach (var role in roles)
            {
                var claims = await roleManager.GetClaimsAsync(role);
                RolesWithClaims.Add(new RoleWithClaims
                {
                    RoleName = role.Name!,
                    Permissions = [.. claims.Where(c => c.Type == "permission").Select(c => c.Value)]
                });
            }
        }
    }

    public class RoleWithClaims
    {
        public string RoleName { get; set; } = "";
        public List<string> Permissions { get; set; } = [];
    }
}
