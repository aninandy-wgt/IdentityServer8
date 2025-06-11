using Duende.IdentityServer;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServerAspNetIdentity;

// Extension methods to configure services and middleware pipeline for the ASP.NET Identity + IdentityServer host.
internal static class HostingExtensions
{
    // Registers all services needed by the application, including EF Core, ASP.NET Identity, and IdentityServer.
    /// <param name="builder">The WebApplicationBuilder for configuring services.</param>
    /// <returns>The built WebApplication instance.</returns>
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // Add Razor Pages support for the built-in UI
        //builder.Services.AddRazorPages();
        builder.Services.AddRazorPages(options =>
        {
            // Require auth everywhere…
            options.Conventions.AuthorizeFolder("/");

            // …except for these Identity pages:
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Logout");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ForgotPassword");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ResetPassword");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ConfirmEmail");
        });

        // Register EF Core with PostgreSQL for the application database
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection")));

        // Configure ASP.NET Identity to use ApplicationUser and IdentityRole
        // Stores user and role data in the EF Core ApplicationDbContext
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Configure IdentityServer with in-memory stores for config data
        builder.Services
            .AddIdentityServer(options =>
            {
                // Enable detailed logging of IdentityServer events for diagnostics
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.UserInteraction.LoginUrl = "/Identity/Account/Login";
                options.UserInteraction.LogoutUrl = "/Identity/Account/Logout";
                options.UserInteraction.ConsentUrl = "/Identity/Consent";
                options.UserInteraction.ErrorUrl = "/Identity/Account/Error";
                options.UserInteraction.DeviceVerificationUrl = "/Identity/Account/Device";

            })
            // Load identity-related resources, API scopes, and client definitions from the Config class
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            // Hook up ASP.NET Identity so IdentityServer can use your ApplicationUser store
            .AddAspNetIdentity<ApplicationUser>()
            // Add the custom profile service so favorite_color gets added as a claim
            .AddProfileService<CustomProfileService>()
            ;

        // Adds default authentication handlers (cookie + OIDC handled by client)
        builder.Services.AddAuthentication();

        // Build and return the WebApplication
        return builder.Build();
    }

    // Configures the middleware pipeline for request handling.
    /// <param name="app">The WebApplication to configure.</param>
    /// <returns>The configured WebApplication.</returns>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Log each HTTP request using Serilog middleware
        app.UseSerilogRequestLogging();

        // In Development, show detailed exception pages
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Serve static files from wwwroot (css, js, images)
        app.UseStaticFiles();

        // Enable routing to match incoming URLs to endpoints
        app.UseRouting();

        // Add IdentityServer endpoints (/connect/authorize, /connect/token, etc.)
        app.UseIdentityServer();

        // Protect API endpoints or Razor pages with authorization
        app.UseAuthorization();

        // Map Razor Pages and require authenticated users for all pages
        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}
