using System.Security.Claims;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServerAspNetIdentity;

public class CustomProfileService : ProfileService<ApplicationUser>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public CustomProfileService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        RoleManager<ApplicationRole> roleManager)
        : base(userManager, claimsFactory)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    protected override async Task GetProfileDataAsync(ProfileDataRequestContext context, ApplicationUser user)
    {
        var principal = await GetUserClaimsAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;

        if (!string.IsNullOrEmpty(user.FavoriteColor))
        {
            identity.AddClaim(new Claim("favorite_color", user.FavoriteColor));
        }

        // Add role and permission claims
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var roleName in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in roleClaims.Where(c => c.Type == "permission"))
                {
                    identity.AddClaim(claim);
                }
            }
        }

        context.AddRequestedClaims(identity.Claims);
    }
}
