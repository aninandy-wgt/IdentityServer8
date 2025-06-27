using Duende.IdentityServer.Models;
using Duende.IdentityModel;

namespace IdentityServerAspNetIdentity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(), new IdentityResources.Profile(),
        new IdentityResource(name: "verification", userClaims: [JwtClaimTypes.Email, JwtClaimTypes.EmailVerified]),
        new IdentityResource(name: "color", displayName: "Your favorite color", userClaims: ["favorite_color"]),
        new IdentityResource(name: "permissions", displayName: "Your permission claims", userClaims: ["permission"]),
        new IdentityResource(name: "roles",displayName: "Your role(s)",userClaims: [JwtClaimTypes.Role])
    ];
    public static IEnumerable<ApiScope> ApiScopes => [
        new ApiScope(name: "api1", displayName: "My API")
        ];
    public static IEnumerable<Client> Clients => [];
    public static IEnumerable<ApiResource> ApiResources { get; } = [];
}
