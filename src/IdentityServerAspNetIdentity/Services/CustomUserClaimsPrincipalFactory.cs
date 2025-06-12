using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace IdentityServerAspNetIdentity.Services;

public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public CustomUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var rc in roleClaims.Where(c => c.Type == "permission"))
                {
                    if (!identity.HasClaim(rc.Type, rc.Value))
                        identity.AddClaim(rc);
                }
            }
        }
        return identity;
    }
}