using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityModel;

namespace IdentityServerAspNetIdentity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources => [new IdentityResources.OpenId(), new IdentityResources.Profile(), new IdentityResource(name: "verification", userClaims: [JwtClaimTypes.Email, JwtClaimTypes.EmailVerified]), new IdentityResource(name: "color", displayName: "Your favorite color", userClaims: ["favorite_color"]), new IdentityResource(name: "permissions", displayName: "Your permission claims", userClaims: ["permission"])];
    public static IEnumerable<ApiScope> ApiScopes => [new ApiScope(name: "api1", displayName: "My API")];
    public static IEnumerable<Client> Clients => [new Client { ClientId = "web", ClientSecrets = { new Secret("secret".Sha256()) }, AllowedGrantTypes = GrantTypes.Code, RedirectUris = { "https://localhost:5002/signin-oidc" }, PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" }, AllowOfflineAccess = true, AllowedScopes = { IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile, "verification", "api1", "color", "offline_access", "permissions" } }];
    public static IEnumerable<ApiResource> ApiResources { get; } = [];
}
