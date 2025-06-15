//using Duende.IdentityServer;
//using Duende.IdentityServer.Models;
//using Duende.IdentityModel;

//namespace IdentityServerAspNetIdentity;

//public static class Config
//{
//    public static IEnumerable<IdentityResource> IdentityResources =>
//        [
//            new IdentityResources.OpenId(),

//            new IdentityResources.Profile(),

//            new() {
//                Name = "verification",
//                UserClaims =
//                [
//                    JwtClaimTypes.Email,           
//                    JwtClaimTypes.EmailVerified    
//                ]
//            },

//            new(
//                name: "color",
//                displayName: "Your favorite color",
//                userClaims: ["favorite_color"])
//            ];

//    public static IEnumerable<ApiScope> ApiScopes =>
//        [
//            new(
//                name: "api1",
//                displayName: "My API")
//        ];

//    public static IEnumerable<Client> Clients =>
//        [
//            new() {
//                ClientId = "client",              
//                AllowedGrantTypes = GrantTypes.ClientCredentials,  
//                ClientSecrets =
//                {
//                    new Secret("secret".Sha256()) 
//                },
//                AllowedScopes = { "api1" }       
//            },

//            new() {
//                ClientId = "web",                
//                ClientSecrets =
//                {
//                    new Secret("secret".Sha256())
//                },
//                AllowedGrantTypes = GrantTypes.Code,               

//                RedirectUris = { "https://localhost:5002/signin-oidc" },
//                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

//                AllowOfflineAccess = true,        

//                AllowedScopes =
//                [
//                    IdentityServerConstants.StandardScopes.OpenId,    
//                    IdentityServerConstants.StandardScopes.Profile,   
//                    "verification",    
//                    "api1",            
//                    "color"            
//                ]
//            }
//        ];
//}
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityModel;

namespace IdentityServerAspNetIdentity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
    
   
    new IdentityResources.OpenId(),    // sub claim
            new IdentityResources.Profile(),   // name, given_name, family_name, etc.

            // your existing custom Email verification resource
            new IdentityResource(
                name: "verification",
                userClaims: new []
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.EmailVerified
                }
            ),

            // your existing favorite–color resource
            new IdentityResource(
                name: "color",
                displayName: "Your favorite color",
                userClaims: new [] { "favorite_color" }
            ),

            // ← NEW: pull any "permission" claims into the UserInfo / ID token
            new IdentityResource(
                name: "permissions",
                displayName: "Your permission claims",
                userClaims: ["permission"]
            )
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope(name: "api1", displayName: "My API")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // machine‐to‐machine client
            new Client
            {
                ClientId          = "client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets     = { new Secret("secret".Sha256()) },
                AllowedScopes     = { "api1" }
            },

            // interactive ASP.NET Core Web App
            new Client
            {
                ClientId            = "web",
                ClientSecrets       = { new Secret("secret".Sha256()) },
                AllowedGrantTypes   = GrantTypes.Code,
                RedirectUris        = { "https://localhost:5002/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
                AllowOfflineAccess  = true,

                // ← UPDATED: now also asks for "permissions"
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "verification",
                    "api1",
                    "color",
                    "offline_access",
                    "permissions"       // ← pull in any "permission" claims
                }
            }
        }
        ; public static IEnumerable<ApiResource> ApiResources { get; } = new List<ApiResource>(); // Added missing property
}
