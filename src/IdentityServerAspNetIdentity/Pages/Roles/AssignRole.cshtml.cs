using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.Pages.Roles;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.ProjectManager}")]
public class AssignRoleModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager) : PageModel
{
    [BindProperty]
    public required string UserId { get; set; }
    
    [BindProperty]
    public required string RoleId { get; set; }

    public required List<ApplicationUser> Users { get; set; }
    public required List<ApplicationRole> Roles { get; set; }
    public Dictionary<string, List<string>> UserRoles { get; set; } = [];

    public async Task OnGetAsync()
    {
        Users = await userManager.Users.ToListAsync();
        Roles = await roleManager.Roles.ToListAsync();

        foreach (var user in Users) UserRoles[user.Id.ToString()] = (await userManager.GetRolesAsync(user)).ToList();
        
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(RoleId)) return Page();

        var user = await userManager.FindByIdAsync(UserId);
        var role = await roleManager.FindByIdAsync(RoleId);

        if (user != null && role != null)
        {
            if (!await userManager.IsInRoleAsync(user, role.Name))
            {
                var result = await userManager.AddToRoleAsync(user, role.Name);
                if (result.Succeeded) return RedirectToPage();

                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRemoveAsync(string userId, string roleName)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var result = await userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded) return RedirectToPage();

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        }
        return Page();
    }
}
