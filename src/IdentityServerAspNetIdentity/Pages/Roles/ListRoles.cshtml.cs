using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerAspNetIdentity.Pages.Roles;

[Authorize(Roles = "AAA_Admin,AAA_Viewer,AAA_ProjectManager")]
public class ListRolesModel(RoleManager<ApplicationRole> roleManager) : PageModel
{
    public List<ApplicationRole> Roles { get; set; } = [];

    public void OnGet() { Roles = [.. roleManager.Roles]; }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role != null)
        {
            var result = await roleManager.DeleteAsync(role);
            if (result.Succeeded) return RedirectToPage();

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        }
        return Page();
    }
}
