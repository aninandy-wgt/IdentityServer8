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
        var principal = await GetUserClaimsAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;

        if (!string.IsNullOrEmpty(user.FavoriteColor)) identity.AddClaim(new Claim("favorite_color", user.FavoriteColor));

        var roles = await userManager.GetRolesAsync(user);
        foreach (var roleName in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await roleManager.GetClaimsAsync(role);
                foreach (var claim in roleClaims.Where(c => c.Type == "permission")) identity.AddClaim(claim);
            }
        }
        context.AddRequestedClaims(identity.Claims);
    }
}
