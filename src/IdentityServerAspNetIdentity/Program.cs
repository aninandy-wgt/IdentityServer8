using Duende.IdentityModel;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.Models;
using IdentityServerAspNetIdentity;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql((string?)(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))));

    builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}").Enrich.FromLogContext().ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityServer Admin API", Version = "v1", Description = "Exposes EF-backed clients, scopes, resources, users, roles and role claims" }); });

    var app = builder.ConfigureServices().ConfigurePipeline();

    if (args.Contains("/seed")) { Log.Information("Seeding database..."); await SeedData.EnsureSeedData(app); Log.Information("Done seeding database. Exiting."); return; }

    app.UseSwagger(); app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityServer API v1"); c.RoutePrefix = "swagger"; });

    app.MapGet("/api/clients", async ([FromServices] ConfigurationDbContext cfg) =>
    {
        return Results.Ok(await cfg.Clients.Include(c => c.AllowedGrantTypes).Include(c => c.AllowedScopes).Include(c => c.RedirectUris).Include(c => c.PostLogoutRedirectUris).Select(c => new
        {
            c.Id,
            c.ClientId,
            c.ClientName,
            AllowedGrantTypes = c.AllowedGrantTypes.Select(gt => gt.GrantType),
            AllowedScopes = c.AllowedScopes.Select(s => s.Scope),
            RedirectUris = c.RedirectUris.Select(r => r.RedirectUri),
            PostLogoutRedirectUris = c.PostLogoutRedirectUris.Select(p => p.PostLogoutRedirectUri),
        }).ToListAsync());
    });

    app.MapPost("/api/clients", async ([FromServices] ConfigurationDbContext cfg, [FromBody] AddClientDto addRequest) =>
    {
        if (string.IsNullOrWhiteSpace(addRequest.ClientId) || string.IsNullOrWhiteSpace(addRequest.Secret)) return Results.BadRequest("ClientId required.");

        if (await cfg.Clients.AnyAsync(c => c.ClientId == addRequest.ClientId)) return Results.Conflict($"Client '{addRequest.ClientId}' already exists.");

        var newClient = new Duende.IdentityServer.EntityFramework.Entities.Client
        {
            ClientId = addRequest.ClientId,
            ClientName = addRequest.ClientName,
            ClientSecrets = [new() { Value = addRequest.Secret.Sha256() }],
            AllowedGrantTypes = [new() { GrantType = GrantTypes.Code.First() }],
            AllowedScopes = addRequest.Scopes?.Select(s => new ClientScope { Scope = s }).ToList() ?? [],
            AllowOfflineAccess = true,
            RedirectUris = addRequest.RedirectUris?.Select(u => new ClientRedirectUri { RedirectUri = u }).ToList() ?? [],
            PostLogoutRedirectUris = addRequest.PostLogoutRedirectUris?.Select(u => new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = u }).ToList() ?? [],
        };

        cfg.Clients.Add(newClient);
        await cfg.SaveChangesAsync();
        return Results.Created($"/api/clients/{newClient.Id}", new { newClient.Id, newClient.ClientId, newClient.ClientName });
    });

    app.MapDelete("/api/clients/{clientId}", async ([FromServices] ConfigurationDbContext cfg, string clientId) =>
    {
        var client = await cfg.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
        if (client == null) return Results.NotFound();
        cfg.Clients.Remove(client);
        await cfg.SaveChangesAsync();
        return Results.Ok();
    });

    app.MapGet("/api/identity-resources", async ([FromServices] ConfigurationDbContext cfg) =>
    {
        return Results.Ok(await cfg.IdentityResources.Include(r => r.UserClaims).Select(r => new
        {
            r.Id,
            r.Name,
            UserClaims = r.UserClaims.Select(uc => uc.Type)
        }).ToListAsync());
    });

    app.MapGet("/api/api-scopes", async ([FromServices] ConfigurationDbContext cfg) =>
    {
        return Results.Ok(await cfg.ApiScopes.Include(s => s.UserClaims).Select(s => new
        {
            s.Id,
            s.Name,
            UserClaims = s.UserClaims.Select(uc => uc.Type)
        }).ToListAsync());
    });

    app.MapGet("/api/resources", async ([FromServices] ConfigurationDbContext cfg) =>
    {
        return Results.Ok(await cfg.ApiResources.Include(r => r.Scopes).Include(r => r.UserClaims).Select(r => new
        {
            r.Id,
            r.Name,
            Scopes = r.Scopes.Select(s => s.Scope),
            UserClaims = r.UserClaims.Select(uc => uc.Type)
        }).ToListAsync());
    });

    app.MapGet("/api/users", async ([FromServices] UserManager<ApplicationUser> userMgr) =>
    {
        return Results.Ok(await userMgr.Users.Select(u => new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.PhoneNumber,
            u.EmailConfirmed,
            u.PhoneNumberConfirmed
        }).ToListAsync());
    });

    app.MapPost("/api/users", async ([FromServices] UserManager<ApplicationUser> userMgr, [FromBody] ApplicationUser newUser) =>
    {
        if (newUser == null || string.IsNullOrEmpty(newUser.UserName) || string.IsNullOrEmpty(newUser.Email)) return Results.BadRequest("Invalid user data.");

        if (await userMgr.FindByNameAsync(newUser.UserName) != null) return Results.Conflict($"User '{newUser.UserName}' already exists.");

        var result = await userMgr.CreateAsync(newUser, "Pass123$");
        if (!result.Succeeded) return Results.BadRequest(result.Errors);

        var claims = new List<Claim>
        {
            new(JwtClaimTypes.GivenName,  newUser.GivenName),
            new(JwtClaimTypes.FamilyName, newUser.FamilyName),
            new("favorite_color",         newUser.FavoriteColor)
        };
        await userMgr.AddClaimsAsync(newUser, claims);

        return Results.Created($"/api/users/{newUser.Id}", new
        {
            newUser.Id,
            newUser.UserName,
            newUser.Email,
            newUser.GivenName,
            newUser.FamilyName,
            newUser.FavoriteColor
        });
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

    app.MapDelete("/api/users/{userId}", async ([FromServices] UserManager<ApplicationUser> userMgr, string userId) =>
    {
        var user = await userMgr.FindByIdAsync(userId);
        if (user == null) return Results.NotFound();

        var result = await userMgr.DeleteAsync(user);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
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
    });//appmapdetete /api/users/{userId}/roles

    app.MapPost("/api/users/{userId}/password", async ([FromServices] UserManager<ApplicationUser> userMgr, string userId, [FromBody] string newPassword) =>
    {
        var user = await userMgr.FindByIdAsync(userId);
        if (user == null) return Results.NotFound();

        var result = await userMgr.ChangePasswordAsync(user, "Pass123$", newPassword);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    });

    app.MapGet("/api/roles", async ([FromServices] RoleManager<ApplicationRole> roleMgr) =>
    {
        return Results.Ok(await roleMgr.Roles.Select(r => new { r.Id, r.Name }).ToListAsync());
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

        return Results.Ok((await roleMgr.GetClaimsAsync(role)).Select(c => new { c.Type, c.Value }));
    });

    app.MapPost("/api/roles/{roleName}/claims", async ([FromServices] RoleManager<ApplicationRole> roleMgr, string roleName, [FromBody] List<AddRoleClaimsDto> addRequest) =>
    {
        var role = await roleMgr.FindByNameAsync(roleName);
        if (role == null) return Results.NotFound();

        if (addRequest == null || addRequest.Count == 0) return Results.BadRequest("No claims provided in the request body.");
        foreach (var claimDto in addRequest)
        {
            if (string.IsNullOrEmpty(claimDto.Type) || string.IsNullOrEmpty(claimDto.Value)) return Results.BadRequest($"Invalid claim data: 'type' or 'value' cannot be empty for one of the claims.");

            if ((await roleMgr.GetClaimsAsync(role)).Any(c => c.Type == claimDto.Type && c.Value == claimDto.Value)) return Results.Conflict($"Claim {claimDto.Type}:{claimDto.Value} already exists in role {roleName}.");

            var result = await roleMgr.AddClaimAsync(role, new Claim(claimDto.Type!, claimDto.Value!));
            if (!result.Succeeded) return Results.BadRequest($"Failed to add claim {claimDto.Type}:{claimDto.Value}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        return Results.Ok("All claims added successfully.");
    });

    app.MapDelete("/api/roles/{roleName}/claims", async ([FromServices] RoleManager<ApplicationRole> roleMgr, string roleName, [FromBody] List<AddRoleClaimsDto> delRequest) =>
    {
        var role = await roleMgr.FindByNameAsync(roleName);
        if (role == null) return Results.NotFound();

        if (delRequest == null || delRequest.Count == 0) return Results.BadRequest("No claims provided in the request body.");
        foreach (var claimDto in delRequest)
        {
            if (string.IsNullOrEmpty(claimDto.Type) || string.IsNullOrEmpty(claimDto.Value)) return Results.BadRequest($"Invalid claim data: 'type' or 'value' cannot be empty for one of the claims.");

            if (!(await roleMgr.GetClaimsAsync(role)).Any(c => c.Type == claimDto.Type && c.Value == claimDto.Value)) return Results.NotFound($"Claim {claimDto.Type}:{claimDto.Value} not found in role {roleName}.");

            var result = await roleMgr.RemoveClaimAsync(role, new Claim(claimDto.Type!, claimDto.Value!));
            if (!result.Succeeded) return Results.BadRequest($"Failed to remove claim {claimDto.Type}:{claimDto.Value}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        return Results.Ok("All claims removed successfully.");
    });

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException) { Log.Fatal(ex, "Unhandled exception"); }
finally { Log.Information("Shut down complete"); Log.CloseAndFlush(); }