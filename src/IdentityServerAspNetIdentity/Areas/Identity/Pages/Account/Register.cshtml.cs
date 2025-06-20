using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace IdentityServerAspNetIdentity.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<RegisterModel> logger) : PageModel
    {
        [BindProperty]
        public required InputModel Input { get; set; }

        public required string ReturnUrl { get; set; }

        public required IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public required string Username { get; set; }

            [Required]
            [Display(Name = "Given Name")]
            public required string GivenName { get; set; }

            [Required]
            [Display(Name = "Family Name")]
            public required string FamilyName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public required string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public required string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public required string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "Favorite Color")]
            public required string FavoriteColor { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Username, Email = Input.Email, GivenName = Input.GivenName, FamilyName = Input.FamilyName, FavoriteColor = Input.FavoriteColor };
                var result = await userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    logger.LogInformation("User created a new account with password.");

                    await userManager.AddClaimsAsync(user, new[]
                    {
                        new Claim(Duende.IdentityModel.JwtClaimTypes.GivenName, user.GivenName ?? ""),
                        new Claim(Duende.IdentityModel.JwtClaimTypes.FamilyName, user.FamilyName ?? ""),
                        new Claim("favorite_color", user.FavoriteColor ?? "")
                    });

                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    _ = Url.Page("/Account/ConfirmEmail", pageHandler: null, values: new { area = "Identity", userId = user.Id, code, returnUrl }, protocol: Request.Scheme);

                    if (userManager.Options.SignIn.RequireConfirmedAccount) return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    else
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}
