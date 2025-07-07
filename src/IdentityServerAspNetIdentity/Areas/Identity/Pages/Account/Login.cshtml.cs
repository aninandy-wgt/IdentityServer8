using System.ComponentModel.DataAnnotations;
using Duende.IdentityServer.Services;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerAspNetIdentity.Areas.Identity.Pages.Account;

[IgnoreAntiforgeryToken]
public class LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger, IIdentityServerInteractionService interaction) : PageModel
{
    [BindProperty]
    public InputModel? Input { get; set; }

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;
        ExternalLogins = [.. await signInManager.GetExternalAuthenticationSchemesAsync()];
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = [.. await signInManager.GetExternalAuthenticationSchemesAsync()];

        if (!ModelState.IsValid) return Page();

        var user = await userManager.FindByNameAsync(Input.Username) ?? await userManager.FindByEmailAsync(Input.Username);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var context = await interaction.GetAuthorizationContextAsync(returnUrl);
            var allowed = (await userManager.GetClaimsAsync(user)).Where(c => c.Type == "allowed_client").Select(c => c.Value);

            if (context != null && !allowed.Contains(context.Client.ClientId))
            {
                await signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "You do not have permission to sign in to this application.");
                return Page();
            }
            logger.LogInformation("User {Username} logged in.", user.UserName);
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor) return RedirectToPage("./LoginWith2fa", (ReturnUrl: returnUrl, Input.RememberMe));
        
        if (result.IsLockedOut)
        {
            logger.LogWarning("User account locked out.");
            return RedirectToPage("./Lockout");
        }
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}
