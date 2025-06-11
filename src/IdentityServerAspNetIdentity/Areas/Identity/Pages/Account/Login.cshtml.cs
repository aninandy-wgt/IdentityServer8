//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Text.Encodings.Web;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using IdentityServerAspNetIdentity.Models;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.Extensions.Logging;

//namespace IdentityServerAspNetIdentity.Areas.Identity.Pages.Account
//{
//    [AllowAnonymous]
//    public class LoginModel : PageModel
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly ILogger<LoginModel> _logger;

//        //public LoginModel(SignInManager<ApplicationUser> signInManager, 
//        //    ILogger<LoginModel> logger,
//        //    UserManager<ApplicationUser> userManager)
//        //{
//        //    _userManager = userManager;
//        //    _signInManager = signInManager;
//        //    _logger = logger;
//        //}

//        [BindProperty]
//        public InputModel Input { get; set; }

//        public IList<AuthenticationScheme> ExternalLogins { get; set; }

//        public string ReturnUrl { get; set; }

//        [TempData]
//        public string ErrorMessage { get; set; }

//        public class InputModel
//        {
//            [Required]
//            [Display(Name = "Username")]
//            public string Username { get; set; }

//            [Required]
//            //[EmailAddress]
//            public string Email { get; set; }

//            [Required]
//            [DataType(DataType.Password)]
//            public string Password { get; set; }

//            [Display(Name = "Remember me?")]
//            public bool RememberMe { get; set; }
//        }

//        public async Task OnGetAsync(string returnUrl = null)
//        {
//            if (!string.IsNullOrEmpty(ErrorMessage))
//            {
//                ModelState.AddModelError(string.Empty, ErrorMessage);
//            }

//            returnUrl ??= Url.Content("~/");

//            // Clear the existing external cookie to ensure a clean login process
//            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

//            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

//            ReturnUrl = returnUrl;
//        }

//        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
//        {
//            returnUrl ??= Url.Content("~/");

//            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

//            if (ModelState.IsValid)
//            {
//                // This doesn't count login failures towards account lockout
//                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
//                //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
//                // try email first, then username
//                var user = await _userManager.FindByEmailAsync(Input.Email)
//                            ?? await _userManager.FindByNameAsync(Input.Username);

//                var result = await _signInManager.PasswordSignInAsync(
//                    user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

//                if (result.Succeeded)
//                {
//                    _logger.LogInformation("User logged in.");
//                    return LocalRedirect(returnUrl);
//                }
//                if (result.RequiresTwoFactor)
//                {
//                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
//                }
//                if (result.IsLockedOut)
//                {
//                    _logger.LogWarning("User account locked out.");
//                    return RedirectToPage("./Lockout");
//                }
//                else
//                {
//                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
//                    return Page();
//                }
//            }

//            // If we got this far, something failed, redisplay form
//            return Page();
//        }
//    }
//}








// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IdentityServerAspNetIdentity.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;




        private readonly UserManager<ApplicationUser> _userManager;

        //public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        //{
        //    _signInManager = signInManager;
        //    _logger = logger;
        //}
        public LoginModel(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,       // ← add this
    ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;                      // ← assign it
            _logger = logger;
        }


        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            //[EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            // Try find by email, then by username:
            var identifier = Input.Email;
            var user = await _userManager.FindByEmailAsync(identifier)
                       ?? await _userManager.FindByNameAsync(identifier);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Now sign in with the found user object:
            var result = await _signInManager.PasswordSignInAsync(
                user, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            if (result.IsLockedOut)
                return RedirectToPage("./Lockout");

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }


        //public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        //{
        //    returnUrl ??= Url.Content("~/");

        //    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        //    if (ModelState.IsValid)
        //    {
        //        // This doesn't count login failures towards account lockout
        //        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        //        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        //        if (result.Succeeded)
        //        {
        //            _logger.LogInformation("User logged in.");
        //            return LocalRedirect(returnUrl);
        //        }
        //        if (result.RequiresTwoFactor)
        //        {
        //            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
        //        }
        //        if (result.IsLockedOut)
        //        {
        //            _logger.LogWarning("User account locked out.");
        //            return RedirectToPage("./Lockout");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        //            return Page();
        //        }
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return Page();
        //}
    }
}
