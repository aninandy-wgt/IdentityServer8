using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Threading.Tasks;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace IdentityServerAspNetIdentity.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender sender) : PageModel
    {
        public required string Email { get; set; }

        public bool DisplayConfirmAccountLink { get; set; }

        public required string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string? returnUrl = null)
        {
            if (email == null) return RedirectToPage("/Index");

            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return NotFound($"Unable to load user with email '{email}'.");
            

            Email = email;
            DisplayConfirmAccountLink = true;
            if (DisplayConfirmAccountLink)
            {
                var userId = await userManager.GetUserIdAsync(user);
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                EmailConfirmationUrl = Url.Page("/Account/ConfirmEmail", pageHandler: null, values: new { area = "Identity", userId, code, returnUrl }, protocol: Request.Scheme);
            }

            return Page();
        }
    }
}
