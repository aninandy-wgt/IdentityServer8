using Duende.IdentityServer.EntityFramework.DbContexts;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using IdentityServerAspNetIdentity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

namespace IdentityServerAspNetIdentity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var migrationsAssembly = typeof(HostingExtensions).Assembly.GetName().Name;

        builder.Services.AddDbContext<ConfigurationDbContext>(options => options.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)));

        builder.Services.AddDbContext<PersistedGrantDbContext>(options => options.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)));

        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

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
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b => b.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(migrationsAssembly));
            })
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<CustomProfileService>();

        builder.Services.AddAuthentication();

        builder.Services.AddAuthorizationBuilder().AddPolicy("AAA_Admin", p => p.RequireRole(AppRoles.Admin)).AddPolicy("AAA_Viewer", p => p.RequireRole(AppRoles.Viewer)).AddPolicy("AAA_ProjectManager", p => p.RequireRole(AppRoles.ProjectManager));

        builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "IdentityServer Admin API",
                Version = "v1",
                Description = "Expose your EF-backed clients, scopes, and resources"
            });
        });

        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Logout");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ForgotPassword");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ResetPassword");
            options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ConfirmEmail");
        });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
        

        app.UseStaticFiles();
        app.UseRouting();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityServer Admin API V1");
            c.RoutePrefix = "swagger";
        });

        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapGet("/api/clients", async ([FromServices] ConfigurationDbContext cfg) =>
        {
            var clients = await cfg.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.AllowedScopes)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .ToListAsync();
            return Results.Ok(clients);
        }).RequireAuthorization("AAA_Admin");

        app.MapGet("/api/identity-resources", async ([FromServices] ConfigurationDbContext cfg) =>
        {
            var idrs = await cfg.IdentityResources
                .Include(r => r.UserClaims)
                .ToListAsync();
            return Results.Ok(idrs);
        })
        .RequireAuthorization("AAA_Admin");

        app.MapGet("/api/api-scopes", async ([FromServices] ConfigurationDbContext cfg) =>
        {
            var scopes = await cfg.ApiScopes
                .Include(s => s.UserClaims)
                .ToListAsync();
            return Results.Ok(scopes);
        })
        .RequireAuthorization("AAA_Admin");

        app.MapControllers();

        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
