using System.Security.Claims;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServerAspNetIdentity;

// Extends the default ASP.NET Identity profile service to include custom user claims.
public class CustomProfileService : ProfileService<ApplicationUser>
{
    // Constructor injects the user manager and claims factory from ASP.NET Identity.
    public CustomProfileService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
        : base(userManager, claimsFactory)
    {
    }

    // Called by IdentityServer when building the identity token or userinfo response.
    // Adds the 'favorite_color' claim if the user has set one.
    /// <param name="context">Context containing requested claim types.</param>
    /// <param name="user">The ApplicationUser instance for whom to supply claims.</param>
    protected override async Task GetProfileDataAsync(ProfileDataRequestContext context, ApplicationUser user)
    {
        // Retrieve the standard claims (sub, name, email, etc.) from ASP.NET Identity
        var principal = await GetUserClaimsAsync(user);

        // The Identity object where we can add custom claims
        var identity = (ClaimsIdentity)principal.Identity!;

        // If the user has specified a favorite color, include it in their claims
        if (!string.IsNullOrEmpty(user.FavoriteColor))
        {
            identity.AddClaim(
                new Claim(
                    type: "favorite_color",         // custom claim type
                    value: user.FavoriteColor));     // value from the user record
        }

        // Only include the claims that the client specifically requested in their scope
        context.AddRequestedClaims(principal.Claims);
    }
}
