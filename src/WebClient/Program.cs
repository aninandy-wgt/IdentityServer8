using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
}).AddCookie("Cookies").AddOpenIdConnect("oidc", static options =>
    {
        options.Authority = "https://localhost:5005";
        options.ClientId = "web";
        options.ClientSecret = "secret";
        options.ResponseType = "code";

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
        options.MapInboundClaims = false;

        options.Scope.Add("offline_access");
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Add("permissions");
        options.ClaimActions.MapJsonKey("permission", "permission");

        options.SaveTokens = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = JwtClaimTypes.Name,
            RoleClaimType = JwtClaimTypes.Role
        };        
    });

builder.Services.AddOpenIdConnectAccessTokenManagement();

builder.Services.AddUserAccessTokenHttpClient("apiClient", configureClient: client =>
{
    client.BaseAddress = new Uri("https://localhost:6001/identity");
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p.RequireRole("AWARE_Admin"))
    .AddPolicy("User", p => p.RequireRole("AWARE_User"));

var app = builder.Build();

if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }

app.UseHttpsRedirection();
//app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages().RequireAuthorization();

app.MapStaticAssets();

app.Run();