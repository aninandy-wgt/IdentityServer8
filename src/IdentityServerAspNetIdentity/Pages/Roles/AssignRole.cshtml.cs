using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.Pages.Admin;

[Authorize(Roles = "AAA_Admin,AAA_ProjectManager")]
public class AssignRoleModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public AssignRoleModel(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty]
    public string UserId { get; set; }
    
    [BindProperty]
    public string RoleId { get; set; }

    public List<ApplicationUser> Users { get; set; }
    public List<ApplicationRole> Roles { get; set; }
    public Dictionary<string, List<string>> UserRoles { get; set; } = new();

    public async Task OnGetAsync()
    {
        Users = await _userManager.Users.ToListAsync();
        Roles = await _roleManager.Roles.ToListAsync();

        foreach (var user in Users)
        {
            UserRoles[user.Id.ToString()] = (await _userManager.GetRolesAsync(user)).ToList();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(RoleId))
            return Page();

        var user = await _userManager.FindByIdAsync(UserId);
        var role = await _roleManager.FindByIdAsync(RoleId);

        if (user != null && role != null)
        {
            if (!await _userManager.IsInRoleAsync(user, role.Name))
            {
                var result = await _userManager.AddToRoleAsync(user, role.Name);
                if (result.Succeeded)
                    return RedirectToPage();

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRemoveAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
                return RedirectToPage();

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
        return Page();
    }
}
