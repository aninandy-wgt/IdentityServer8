
# DemoApp IdentityServer Integration Guide

Below is a detailed, step-by-step guide to registering a new client in IdentityServer and wiring it up in your DemoApp’s `Program.cs`.
## 1. Registering a New Client in IdentityServer

In the IdentityServer Swagger UI, open the **POST/api/clients** endpoint and paste the following JSON payload to register your new application:
````json
{
  "clientId": "<your-client-id>",   // replace with own client id (ex. "daaa")
  "clientName": null,       
  "secret": "secret",
  "redirectUris": [
    "https://localhost:7006/signin-oidc"   // replace with app's base URL port (applicationUrl)
  ],
  "postLogoutRedirectUris": [
    "https://localhost:7006/signout-callback-oidc"   // same port as above
  ], 
  "scopes": [
    "openid",
    "profile",
    "roles",
    "verification",
    "api1",
    "color",
    "offline_access",
    "permissions"
  ]
}
````
| Property | Description |
| :------- | :---------- |
| **clientId** | A unique identifier for your app—change to match your app’s name (e.g. `DemoApp`). |
| **secret** | Shared secret for the code-flow back channel. Must match `options.ClientSecret` in your app. |
| **redirectUris** | Where IdentityServer sends the OIDC response (the `/signin-oidc` endpoint in your app). |
| **postLogoutRedirectUris** | Where to return the browser after sign-out completes (the `/signout-callback-oidc` endpoint). |
| **scopes** | Which scopes (claims & APIs) the client may request: |

* `openid`, `profile` → standard OIDC claims
* `roles`, `verification`, `color`, `permissions` → custom identity/scopes
* `api1` → protected API scope
* `offline_access` → allows issuance of a refresh token  |

> **Note:** To persist a refresh-token grant record under `/grants`, include `"offline_access"` in scopes **and** set `AllowOfflineAccess = true` on the `Client` object.
---
## 2. Wiring Up the DemoApp (`Program.cs`)
```csharp
using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1) MVC + RazorPages
builder.Services.AddRazorPages();

// 2) Cookie + OIDC authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies")
.AddOpenIdConnect("oidc", options => {
    // IdentityServer endpoint
    options.Authority = "https://localhost:5005";

    // Client identity—must match your JSON above
    options.ClientId     = "daaa";
    options.ClientSecret = "secret";
    options.ResponseType = "code";

    // Scopes & claim mappings
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");

    options.Scope.Add("roles");
    options.ClaimActions.MapJsonKey("role", "role");

    options.Scope.Add("verification");
    options.ClaimActions.MapJsonKey("email_verified", "email_verified");

    options.Scope.Add("api1");

    options.Scope.Add("color");
    options.ClaimActions.MapUniqueJsonKey("favorite_color", "favorite_color");

    // Include offline_access to enable refresh tokens
    options.Scope.Add("offline_access");

    options.Scope.Add("permissions");
    options.ClaimActions.MapJsonKey("permission", "permission");

    options.GetClaimsFromUserInfoEndpoint = true;
    options.SaveTokens = true;

    // Map claim types for ASP.NET Core
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = JwtClaimTypes.Name,
        RoleClaimType = JwtClaimTypes.Role
    };
});

// 3) Automatic token management (refresh, etc.)
builder.Services.AddOpenIdConnectAccessTokenManagement();

// 4) Typed HttpClient with access token
builder.Services.AddUserAccessTokenHttpClient("apiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:6001/identity");
});

// 5) Authorization policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p.RequireRole("AWARE_Admin"))
    .AddPolicy("User",  p => p.RequireRole("AWARE_User"));

var app = builder.Build();

// 6) Middleware pipeline
i
f (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();
app.MapStaticAssets();
app.Run();
```
With this file in place— IdentityServer client JSON registration and matching DemoApp configuration—`daaa` client will be fully integrated, appear under `/grants`, and support sign-in, refresh tokens, and role-based authorization end to end.
