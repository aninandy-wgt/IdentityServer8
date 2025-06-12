using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using IdentityServerAspNetIdentity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServerAspNetIdentity;
internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/");

            options.Conventions.AuthorizeFolder("/Roles", "AAA_Admin");
            
            options.Conventions.AllowAnonymousToPage("/Roles/ListRoles"); 
            options.Conventions.AuthorizePage("/Roles/ListRoles", "AAA_Viewer");

            options.Conventions.AllowAnonymousToPage("/Roles/ListRoles");
            options.Conventions.AuthorizePage("/Roles/ListRoles", "AAA_ProjectManager");
            options.Conventions.AllowAnonymousToPage("/Roles/AssignRole");
            options.Conventions.AuthorizePage("/Roles/AssignRole", "AAA_ProjectManager");

            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Logout");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ForgotPassword");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ResetPassword");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ConfirmEmail");
        });

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddIdentityServer(options =>
            {
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
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<CustomProfileService>()
            ;

        builder.Services.AddAuthentication();

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("AAA_Admin", configurePolicy: policy =>
            {
                policy.RequireRole(AppRoles.Admin);
            }).AddPolicy("AAA_Viewer", configurePolicy: policy =>
            {
                policy.RequireRole(AppRoles.Viewer);
            }).AddPolicy("AAA_ProjectManager", configurePolicy: policy =>
            {
                policy.RequireRole(AppRoles.ProjectManager);
            });

        builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseIdentityServer();

        app.UseAuthorization();

        app.MapControllers(); 

        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
