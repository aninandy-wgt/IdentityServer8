using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityModel;

namespace IdentityServerAspNetIdentity;

public static class Config
{
    // Identity resources represent user data that client apps can request.
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            // Standard subject identifier (user ID) resource
            new IdentityResources.OpenId(),

            // Standard profile data (name, birthdate, etc.)
            new IdentityResources.Profile(),

            // Custom "verification" resource to include email and verification status
            new IdentityResource
            {
                Name = "verification",
                UserClaims = new List<string>
                {
                    JwtClaimTypes.Email,           // user email address
                    JwtClaimTypes.EmailVerified    // whether email is confirmed
                }
            },

            // Custom "color" resource to carry a favorite_color claim
            new IdentityResource(
    name: "color",
    displayName: "Your favorite color",
    userClaims: new[] { "favorite_color" }
)
        };

    // API scopes represent the APIs this IdentityServer protects.
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            // "api1" is a name clients will use to ask for access to "My API"
            new ApiScope(
                name: "api1",
                displayName: "My API")
        };

    // Clients are applications that want to access resources (identity or APIs).
    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // Machine-to-machine client (no user involved)
            new Client
            {
                ClientId = "client",              // unique ID for this client
                AllowedGrantTypes = GrantTypes.ClientCredentials,  // use client ID/secret
                ClientSecrets =
                {
                    new Secret("secret".Sha256()) // secret shared between this client and server
                },
                AllowedScopes = { "api1" }       // this client can call "api1" only
            },

            // Interactive web application using OpenID Connect (user logs in)
            new Client
            {
                ClientId = "web",                // unique ID for the web application
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedGrantTypes = GrantTypes.Code,               // authorization code flow

                // URLs where the user will be sent back after login/logout
                RedirectUris = { "https://localhost:5002/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                AllowOfflineAccess = true,        // allow refresh tokens

                // scopes this app can request:
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,    // user identifier
                    IdentityServerConstants.StandardScopes.Profile,   // basic profile data
                    "verification",    // email and email_verified claims
                    "api1",            // access to the protected API
                    "color"            // favorite_color custom claim
                }
            }
        };
}
