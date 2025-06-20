using Microsoft.AspNetCore.Authentication;
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

        options.Scope.Add("verification");
        options.ClaimActions.MapJsonKey("email_verified", "email_verified");

        options.Scope.Add("api1");

        options.Scope.Add("offline_access");

        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Add("color");
        options.ClaimActions.MapUniqueJsonKey("favorite_color", "favorite_color");

        options.MapInboundClaims = false; 

        options.Scope.Add("permissions");
        options.GetClaimsFromUserInfoEndpoint = true;
        options.ClaimActions.MapJsonKey("permission", "permission");

        options.SaveTokens = true;
    });
builder.Services.AddOpenIdConnectAccessTokenManagement();

builder.Services.AddUserAccessTokenHttpClient("apiClient", configureClient: client =>
{
    client.BaseAddress = new Uri("https://localhost:6001/identity");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages().RequireAuthorization();

app.MapStaticAssets();
//app.MapRazorPages().WithStaticAssets();

app.Run();