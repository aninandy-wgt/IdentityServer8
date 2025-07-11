using Duende.IdentityModel;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace IdentityServerAspNetIdentity;

public static class SeedData
{
    public static async Task EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.Migrate();

        var configDb = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        configDb.Database.Migrate();

        scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var r in (string[])[AppRoles.Admin, AppRoles.ProjectManager, AppRoles.Viewer])
        {
            if (!await roleMgr.RoleExistsAsync(r))
            {
                var roleResult = await roleMgr.CreateAsync(new ApplicationRole { Name = r });
                if (!roleResult.Succeeded) throw new Exception(roleResult.Errors.First().Description);
            }
        }

        await AddRoleClaims(roleMgr, AppRoles.Admin, [AppPermissions.ListRoles, AppPermissions.CreateRole, AppPermissions.AssignRole]);
        await AddRoleClaims(roleMgr, AppRoles.ProjectManager, [AppPermissions.ListRoles, AppPermissions.AssignRole]);
        await AddRoleClaims(roleMgr, AppRoles.Viewer, [AppPermissions.ListRoles]);

        await SeedUser(userMgr, "alice", "AliceSmith@example.com", "Pass123$", "red", "Alice", "Smith", (JwtClaimTypes.GivenName, "Alice"), (JwtClaimTypes.FamilyName, "Smith"), (JwtClaimTypes.WebSite, "http://alice.example.com"), ("location", "WGT CA office"));
        await SeedUser(userMgr, "bob", "BobSmith@example.com", "Pass123$", "blue", "Bob", "Smith", (JwtClaimTypes.GivenName, "Bob"), (JwtClaimTypes.FamilyName, "Smith"), (JwtClaimTypes.WebSite, "http://bob.example.com"), ("location", "WGT CA office"));
        await SeedUser(userMgr, "tom", "tomsmith@example.com", "Pass123$", "green", "Tom", "Smith", (JwtClaimTypes.GivenName, "Tom"), (JwtClaimTypes.FamilyName, "Smith"), (JwtClaimTypes.WebSite, "http://tom.example.com"), ("location", "WGT SA office"));
        await SeedUser(userMgr, "john", "johnsmith@example.com", "Pass123$", "purple", "John", "Smith", (JwtClaimTypes.GivenName, "John"), (JwtClaimTypes.FamilyName, "Smith"), (JwtClaimTypes.WebSite, "http://john.example.com"), ("location", "WGT CA office"));

        var alice = await userMgr.FindByNameAsync("alice");
        if (!await userMgr.IsInRoleAsync(alice, AppRoles.Admin))
        {
            var addRoleResult = await userMgr.AddToRoleAsync(alice, AppRoles.Admin);
            if (!addRoleResult.Succeeded) throw new Exception(addRoleResult.Errors.First().Description);
        }

        await EnsureIdentityServerSeedData(configDb);

        Log.Debug("Seeding complete: roles, users, and IdentityServer configuration created.");
    }

    private static async Task AddRoleClaims(RoleManager<ApplicationRole> roleMgr, string roleName, string[] permissions)
    {
        var role = await roleMgr.FindByNameAsync(roleName);
        if (role != null) foreach (var permission in permissions) if (!(await roleMgr.GetClaimsAsync(role)).Any(c => c.Type == "permission" && c.Value == permission)) await roleMgr.AddClaimAsync(role, new Claim("permission", permission));
    }

    private static async Task SeedUser(UserManager<ApplicationUser> userMgr, string userName, string email, string password, string favoriteColor, string givenName, string familyName, params (string Type, string Value)[] extraClaims)
    {
        var user = await userMgr.FindByNameAsync(userName);
        if (user == null)
        {
            user = new ApplicationUser { UserName = userName, Email = email, EmailConfirmed = true, FavoriteColor = favoriteColor, GivenName = givenName, FamilyName = familyName };
            var createResult = await userMgr.CreateAsync(user, password);
            if (!createResult.Succeeded) throw new Exception(createResult.Errors.First().Description);

            var claimResult = await userMgr.AddClaimsAsync(user, [.. extraClaims.Select(c => new Claim(c.Type, c.Value))]);
            if (!claimResult.Succeeded) throw new Exception(claimResult.Errors.First().Description);

            Log.Debug($"{userName} created");
        }
        else
        {
            bool mustUpdate = false;
            if (user.GivenName != givenName) { user.GivenName = givenName; mustUpdate = true; }
            if (user.FamilyName != familyName) { user.FamilyName = familyName; mustUpdate = true; }
            if (mustUpdate)
            {
                var updateResult = await userMgr.UpdateAsync(user);
                if (!updateResult.Succeeded) throw new Exception(updateResult.Errors.First().Description);
            }
            Log.Debug($"{userName} already exists");
        }
    }

    public static async Task EnsureIdentityServerSeedData(ConfigurationDbContext context)
    {
        if (!context.Clients.Any())
        {
            foreach (var client in Config.Clients) context.Clients.Add(client.ToEntity());
            await context.SaveChangesAsync();
        }

        if (!context.IdentityResources.Any())
        {
            foreach (var resource in Config.IdentityResources) context.IdentityResources.Add(resource.ToEntity());
            await context.SaveChangesAsync();
        }

        if (!context.ApiScopes.Any())
        {
            foreach (var apiScope in Config.ApiScopes) context.ApiScopes.Add(apiScope.ToEntity());
            await context.SaveChangesAsync();
        }

        if (Config.ApiResources != null && !context.ApiResources.Any())
        {
            foreach (var apiResource in Config.ApiResources) context.ApiResources.Add(apiResource.ToEntity());
            await context.SaveChangesAsync();
        }
    }
}