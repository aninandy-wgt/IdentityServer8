using Duende.IdentityServer.EntityFramework.DbContexts;
using IdentityServerAspNetIdentity;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.AspNetCore.Mvc;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

    builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}").Enrich.FromLogContext().ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityServer Admin API", Version = "v1", Description = "Exposes EF-backed clients, scopes, resources, users, user claims, roles and role claims" }); });

    var app = builder.ConfigureServices().ConfigurePipeline();

    if (args.Contains("/seed")) { Log.Information("Seeding database..."); await SeedData.EnsureSeedData(app); Log.Information("Done seeding database. Exiting."); return; }

    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityServer API v1"); c.RoutePrefix = "swagger"; });

    app.MapGet("/api/clients", async ([FromServices] ConfigurationDbContext cfg) =>
    {
        var clients = await cfg.Clients.Include(c => c.AllowedGrantTypes).Include(c => c.AllowedScopes).Include(c => c.RedirectUris).Include(c => c.PostLogoutRedirectUris).Select(c => new
            {
                c.Id,
                c.ClientId,
                c.ClientName,
                AllowedGrantTypes = c.AllowedGrantTypes.Select(gt => gt.GrantType),
                AllowedScopes = c.AllowedScopes.Select(s => s.Scope),
                RedirectUris = c.RedirectUris.Select(r => r.RedirectUri),
                PostLogoutRedirectUris = c.PostLogoutRedirectUris.Select(p => p.PostLogoutRedirectUri),
            }).ToListAsync(); return Results.Ok(clients);
    });

    app.MapGet("/api/identity-resources", async ([FromServices] ConfigurationDbContext cfg) =>
    {
        var idrs = await cfg.IdentityResources.Include(r => r.UserClaims).Select(r => new
            {
                r.Id,
                r.Name,
                UserClaims = r.UserClaims.Select(uc => uc.Type)
            }).ToListAsync(); return Results.Ok(idrs);
    });

    app.MapGet("/api/api-scopes", async ([FromServices] ConfigurationDbContext cfg) =>
    {
        var scopes = await cfg.ApiScopes.Include(s => s.UserClaims).Select(s => new
            {
                s.Id,
                s.Name,
                UserClaims = s.UserClaims.Select(uc => uc.Type)
            }).ToListAsync(); return Results.Ok(scopes);
    });

    app.MapGet("/api/resources", async ([FromServices] ConfigurationDbContext cfg) =>
    {
        var resources = await cfg.ApiResources.Include(r => r.Scopes).Include(r => r.UserClaims).Select(r => new
            {
                r.Id,
                r.Name,
                Scopes = r.Scopes.Select(s => s.Scope),
                UserClaims = r.UserClaims.Select(uc => uc.Type)
        }).ToListAsync(); return Results.Ok(resources);
    });

    app.MapGet("/api/users", async ([FromServices] UserManager<ApplicationUser> userMgr) =>
    {
        var users = await userMgr.Users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.PhoneNumber,
                u.EmailConfirmed,
                u.PhoneNumberConfirmed
            })
            .ToListAsync(); return Results.Ok(users);
    });

    app.MapGet("/api/users/{userId}", async ([FromServices] UserManager<ApplicationUser> userMgr, string userId) =>
    {
        var user = await userMgr.FindByIdAsync(userId);
        return user == null ? Results.NotFound() : Results.Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.EmailConfirmed,
                user.PhoneNumberConfirmed
            });
    });

    app.MapGet("/api/users/{userId}/roles", async ([FromServices] UserManager<ApplicationUser> userMgr, string userId) =>
    {
        var user = await userMgr.FindByIdAsync(userId);
        return user == null ? Results.NotFound() : Results.Ok(await userMgr.GetRolesAsync(user));
    });

    app.MapPost("/api/users/{userId}/roles", async ([FromServices] UserManager<ApplicationUser> userMgr, string userId, [FromBody] string roleName) =>
    {
        var user = await userMgr.FindByIdAsync(userId);
        if (user == null) return Results.NotFound();

        var result = await userMgr.AddToRoleAsync(user, roleName);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    });

    app.MapPost("/api/users", async ([FromServices] UserManager<ApplicationUser> userMgr, [FromBody] ApplicationUser newUser) =>
    {
        if (newUser == null || string.IsNullOrEmpty(newUser.UserName) || string.IsNullOrEmpty(newUser.Email)) return Results.BadRequest("Invalid user data.");

        if (await userMgr.FindByNameAsync(newUser.UserName) != null) return Results.Conflict($"User '{newUser.UserName}' already exists.");
        
        var result = await userMgr.CreateAsync(newUser, "DefaultPassword123!");
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    });

    app.MapDelete("/api/users/{userId}", async ([FromServices] UserManager<ApplicationUser> userMgr, string userId) =>
    {
        var user = await userMgr.FindByIdAsync(userId);
        if (user == null) return Results.NotFound();

        var result = await userMgr.DeleteAsync(user);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    });

    app.MapPost("/api/users/{userId}/password", async ([FromServices] UserManager<ApplicationUser> userMgr, string userId, [FromBody] string newPassword) =>
    {
        var user = await userMgr.FindByIdAsync(userId);
        if (user == null) return Results.NotFound();

        var result = await userMgr.ChangePasswordAsync(user, "DefaultPassword123!", newPassword);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    });

    app.MapGet("/api/roles", async ([FromServices] RoleManager<ApplicationRole> roleMgr) =>
    {
        var roles = await roleMgr.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
        return Results.Ok(roles);
    });

    app.MapPost("/api/roles", async ([FromServices] RoleManager<ApplicationRole> roleMgr, [FromBody] string roleName) =>
    {
        if (await roleMgr.RoleExistsAsync(roleName)) return Results.Conflict($"Role '{roleName}' already exists.");
        
        var result = await roleMgr.CreateAsync(new ApplicationRole { Name = roleName });
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    });

    app.MapDelete("/api/roles/{roleName}", async ([FromServices] RoleManager<ApplicationRole> roleMgr, string roleName) =>
    {
        var role = await roleMgr.FindByNameAsync(roleName);
        if (role == null) return Results.NotFound();
        
        var result = await roleMgr.DeleteAsync(role);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    });

    app.MapGet("/api/roles/{roleName}/claims", async ([FromServices] RoleManager<ApplicationRole> roleMgr, string roleName) =>
    {
        var role = await roleMgr.FindByNameAsync(roleName);
        if (role == null) return Results.NotFound();
        
        var claims = await roleMgr.GetClaimsAsync(role);
        return Results.Ok(claims.Select(c => new { c.Type, c.Value }));
    });

    app.MapPost("/api/roles/{roleName}/claims", async ([FromServices] RoleManager<ApplicationRole> roleMgr, string roleName, [FromBody] Dictionary<string, string> body) =>
    {
        if (!body.TryGetValue("type", out var type) || !body.TryGetValue("value", out var value)) return Results.BadRequest("Missing 'type' or 'value'.");

        var role = await roleMgr.FindByNameAsync(roleName);
        if (role == null) return Results.NotFound();

        var result = await roleMgr.AddClaimAsync(role, new System.Security.Claims.Claim(type, value));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    });

    app.MapDelete("/api/roles/{roleName}/claims", async ([FromServices] RoleManager<ApplicationRole> roleMgr, string roleName, [FromBody] Dictionary<string, string> body) =>
    {
        if (!body.TryGetValue("type", out var type) || !body.TryGetValue("value", out var value)) return Results.BadRequest("Missing 'type' or 'value'.");

        var role = await roleMgr.FindByNameAsync(roleName);
        if (role == null) return Results.NotFound();

        var result = await roleMgr.RemoveClaimAsync(role, new System.Security.Claims.Claim(type, value));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    });

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException) { Log.Fatal(ex, "Unhandled exception"); }
finally { Log.Information("Shut down complete"); Log.CloseAndFlush(); }