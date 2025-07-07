using System.Security.Claims;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServerAspNetIdentity;

public class CustomProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, RoleManager<ApplicationRole> roleManager) : ProfileService<ApplicationUser>(userManager, claimsFactory)
{
    protected override async Task GetProfileDataAsync(ProfileDataRequestContext context, ApplicationUser user)
    {
        var identity = (ClaimsIdentity)(await GetUserClaimsAsync(user)).Identity!;

        if (!string.IsNullOrEmpty(user.FavoriteColor)) identity.AddClaim(new Claim("favorite_color", user.FavoriteColor));

        foreach (var roleName in (IList<string>?)await userManager.GetRolesAsync(user))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null) foreach (var claim in (await roleManager.GetClaimsAsync(role)).Where(c => c.Type == "permission")) identity.AddClaim(claim);
        }
        context.AddRequestedClaims(identity.Claims);
    }
}
