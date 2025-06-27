````markdown
# DemoApp IdentityServer Integration Guide

Below is a detailed, step-by-step guide to registering a new client in IdentityServer and wiring it up in your DemoApp’s `Program.cs`.

---

## 1. Registering a New Client in IdentityServer

In the IdentityServer Swagger UI, open the **Create Client** endpoint and paste the following JSON payload to register your new application:

```json
{
  "clientId": "<your-client-id>",   // replace with your own client identifier (e.g., "daaa")
  "clientName": "<your-app-name>",   // replace with your application's display name         // human-friendly name; clearly distinct from clientId
  "secret": "secret",
  "redirectUris": [
    "https://localhost:7006/signin-oidc"   // replace 7006 with your app's base URL port (applicationUrl)
  ],
  "postLogoutRedirectUris": [
    "https://localhost:7006/signout-callback-oidc"   // same port as above; matches your applicationUrl
  ],
  "postLogoutRedirectUris": [
    "https://localhost:7006/signout-callback-oidc"
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
| -------- | ----------- |
|          |             |

| **clientId**               | A unique identifier for your app—change to match your app’s name (e.g. `DemoApp`).            |
| -------------------------- | --------------------------------------------------------------------------------------------- |
| **clientName**             | A human-friendly display name for the consent screen (optional).                              |
| **secret**                 | Shared secret for the code-flow back channel. Must match `options.ClientSecret` in your app.  |
| **redirectUris**           | Where IdentityServer sends the OIDC response (the `/signin-oidc` endpoint in your app).       |
| **postLogoutRedirectUris** | Where to return the browser after sign-out completes (the `/signout-callback-oidc` endpoint). |
| **scopes**                 | Which scopes (claims & APIs) the client may request:                                          |

* `openid`, `profile` → standard OIDC claims
* `roles`, `verification`, `color`, `permissions` → custom identity/scopes
* `api1` → protected API scope
* `offline_access` → allows issuance of a refresh token  |

> **Note:** To persist a refresh-token grant record under `/grants`, include `"offline_access"` in scopes **and** set `AllowOfflineAccess = true` on the `Client` object.

---

## 2. Wiring Up the DemoApp (`Program.cs`)

Below is a walkthrough of each section in your `Program.cs`:

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

### Key Points

1. `** / **`

   * Sets up OIDC code flow (`ResponseType = "code"`) and cookie session.

2. **Scopes & Claims**

   * Call `options.Scope.Add(...)` for every server-side scope.
   * Use `MapJsonKey` / `MapUniqueJsonKey` to pull custom claims into your user principal.

3. \`\`

   * Adding this scope + `AddOpenIdConnectAccessTokenManagement()` enables silent token renewal and persists a grant entry under **/grants**.

4. **Authorization Policies**

   * Define `Admin` / `User` roles to secure pages or API calls.

With this file in place—your IdentityServer client JSON registration and matching DemoApp configuration—your `daaa` client will be fully integrated, appear under `/grants`, and support sign-in, refresh tokens, and role-based authorization end to end.

```
```