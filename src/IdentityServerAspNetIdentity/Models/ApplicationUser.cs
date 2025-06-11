using Microsoft.AspNetCore.Identity;

namespace IdentityServerAspNetIdentity.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser<int>
{
    public string? FavoriteColor { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
}

public class ApplicationRole : IdentityRole<int>
{
    // Add custom properties if needed
}
