using Microsoft.AspNetCore.Identity;

namespace IdentityServerAspNetIdentity.Models;

public class ApplicationUser : IdentityUser<int>
{
    public string? FavoriteColor { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
}

public class ApplicationRole : IdentityRole<int> { }
