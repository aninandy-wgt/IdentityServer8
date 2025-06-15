using Microsoft.AspNetCore.Authentication;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
    .AddCookie("Cookies")
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:5001";

        //AdminUI
        //options.Authority = "http://localhost:3000/ids";
        //options.RequireHttpsMetadata = false;

        //QS2:matches IdentityServer config
        options.ClientId = "web";
        options.ClientSecret = "secret";

        options.ResponseType = "code";

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");

        //QS2:web client asking to access the "verfication" scope
        options.Scope.Add("verification");
        options.ClaimActions.MapJsonKey("email_verified", "email_verified");

        //QS3:web client asking to access the "api1" scope
        options.Scope.Add("api1");

        //QS3a:web client asking to access refresh token
        options.Scope.Add("offline_access");

        options.GetClaimsFromUserInfoEndpoint = true;// gets the user name and other info

        //need to add this
        options.Scope.Add("color");
        options.ClaimActions.MapUniqueJsonKey("favorite_color", "favorite_color");

        options.MapInboundClaims = false; // Don't rename claim types

        options.Scope.Add("permissions");
        options.GetClaimsFromUserInfoEndpoint = true;
        options.ClaimActions.MapJsonKey("permission", "permission");

        options.SaveTokens = true;
    });
builder.Services.AddOpenIdConnectAccessTokenManagement();

//QS3a:to resuse HttpClient instances
builder.Services.AddUserAccessTokenHttpClient("apiClient", configureClient: client =>
{
    client.BaseAddress = new Uri("https://localhost:6001");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages().RequireAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();