using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerAspNetIdentity.Pages.Admin;

[Authorize(Roles = "AAA_Admin,AAA_Viewer,AAA_ProjectManager")]//AAA_viewer not letting see roles
public class ListRolesModel : PageModel
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public List<ApplicationRole> Roles { get; set; } = [];

    public ListRolesModel(RoleManager<ApplicationRole> roleManager) => _roleManager = roleManager;

    public void OnGet()
    {
        Roles = [.. _roleManager.Roles];
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role != null)
        {
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return RedirectToPage();

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
        return Page();
    }
}
