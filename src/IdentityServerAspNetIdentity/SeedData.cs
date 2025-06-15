//using Duende.IdentityModel;
//using IdentityServerAspNetIdentity.Data;
//using IdentityServerAspNetIdentity.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Serilog;
//using System.Security.Claims;

//namespace IdentityServerAspNetIdentity
//{
//    public static class SeedData
//    {
//        public static async Task EnsureSeedData(WebApplication app)
//        {
//            using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
//            var sp = scope.ServiceProvider;
//            var db = sp.GetRequiredService<ApplicationDbContext>();
//            var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
//            var roleMgr = sp.GetRequiredService<RoleManager<ApplicationRole>>();

//            // 1) Migrate database
//            db.Database.EnsureDeleted();
//            db.Database.Migrate();

//            // 2) Seed roles
//            string[] roles = { AppRoles.Admin, AppRoles.ProjectManager, AppRoles.Viewer };
//            foreach (var r in roles)
//            {
//                var exists = roleMgr.RoleExistsAsync(r).Result;
//                if (!exists)
//                {
//                    var roleResult = roleMgr.CreateAsync(new ApplicationRole { Name = r }).Result;
//                    if (!roleResult.Succeeded)
//                        throw new Exception(roleResult.Errors.First().Description);
//                }
//            }

//            // 2a) Add claims to roles
//            var adminRole = await roleMgr.FindByNameAsync(AppRoles.Admin);
//            if (adminRole != null)
//            {
//                var existingClaims = await roleMgr.GetClaimsAsync(adminRole);
//                var permissions = new[]
//                {
//                    AppPermissions.ListRoles,
//                    AppPermissions.CreateRole,
//                    AppPermissions.AssignRole
//                };
//                foreach (var permission in permissions)
//                {
//                    if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
//                    {
//                        await roleMgr.AddClaimAsync(adminRole, new Claim("permission", permission));
//                    }
//                }
//            }

//            var pmRole = await roleMgr.FindByNameAsync(AppRoles.ProjectManager);
//            if (pmRole != null)
//            {
//                var existingClaims = await roleMgr.GetClaimsAsync(pmRole);
//                var permissions = new[]
//                {
//                    AppPermissions.ListRoles,
//                    AppPermissions.AssignRole
//                };
//                foreach (var permission in permissions)
//                {
//                    if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
//                    {
//                        await roleMgr.AddClaimAsync(pmRole, new Claim("permission", permission));
//                    }
//                }
//            }

//            var viewerRole = await roleMgr.FindByNameAsync(AppRoles.Viewer);
//            if (viewerRole != null) {
//                var existingClaims = await roleMgr.GetClaimsAsync(viewerRole);
//                var permissions = new[]
//                {
//                    AppPermissions.ListRoles
//                };
//                foreach (var permission in permissions)
//                {
//                    if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
//                    {
//                        await roleMgr.AddClaimAsync(viewerRole, new Claim("permission", permission));
//                    }
//                }
//            }

//            // 3) Seed users and their properties/claims
//            SeedUser(userMgr,
//                userName: "alice",
//                email: "AliceSmith@example.com",
//                password: "Pass123$",
//                favoriteColor: "red",
//                givenName: "Alice",
//                familyName: "Smith",
//                (JwtClaimTypes.GivenName, "Alice"),
//                (JwtClaimTypes.FamilyName, "Smith"),
//                (JwtClaimTypes.WebSite, "http://alice.example.com"),
//                ("location", "WGT CA office")
//            );

//            SeedUser(userMgr,
//                userName: "bob",
//                email: "BobSmith@example.com",
//                password: "Pass123$",
//                favoriteColor: "blue",
//                givenName: "Bob",
//                familyName: "Smith", (JwtClaimTypes.GivenName, "Bob"),
//                (JwtClaimTypes.FamilyName, "Smith"),                
//                (JwtClaimTypes.WebSite, "http://bob.example.com"),
//                ("location", "WGT CA office")
//            );

//            SeedUser(userMgr,
//                userName: "tom",
//                email: "tomsmith@example.com",
//                password: "Pass123$",
//                favoriteColor: "green",
//                givenName: "Tom",
//                familyName: "Smith", (JwtClaimTypes.GivenName, "Tom"),
//                (JwtClaimTypes.FamilyName, "Smith"),
//                (JwtClaimTypes.WebSite, "http://tom.example.com"),
//                ("location", "WGT SA office")
//            );

//            SeedUser(userMgr,
//                userName: "john",
//                email: "johnsmith@example.com",
//                password: "Pass123$",
//                favoriteColor: "purple",
//                givenName: "John",
//                familyName: "Smith", (JwtClaimTypes.GivenName, "John"),
//                (JwtClaimTypes.FamilyName, "Smith"),
//                (JwtClaimTypes.WebSite, "http://john.example.com"),
//                ("location", "WGT CA office")
//            );

//            // 4) Assign Alice to Admin role
//            var alice = userMgr.FindByNameAsync("alice").Result;
//            if (!userMgr.IsInRoleAsync(alice, AppRoles.Admin).Result)
//            {
//                var addRoleResult = userMgr.AddToRoleAsync(alice, AppRoles.Admin).Result;
//                if (!addRoleResult.Succeeded)
//                    throw new Exception(addRoleResult.Errors.First().Description);
//            }

//            Log.Debug("Seeding complete: roles created and users assigned.");
//        }

//        private static void SeedUser(
//            UserManager<ApplicationUser> userMgr,
//            string userName,
//            string email,
//            string password,
//            string favoriteColor,
//            string givenName,
//            string familyName,
//            params (string Type, string Value)[] extraClaims)
//        {
//            var user = userMgr.FindByNameAsync(userName).Result;
//            if (user == null)
//            {
//                user = new ApplicationUser
//                {
//                    UserName = userName,
//                    Email = email,
//                    EmailConfirmed = true,
//                    FavoriteColor = favoriteColor,
//                    GivenName = givenName,
//                    FamilyName = familyName
//                };
//                var createResult = userMgr.CreateAsync(user, password).Result;
//                if (!createResult.Succeeded)
//                    throw new Exception(createResult.Errors.First().Description);

//                var claimList = extraClaims.Select(c => new Claim(c.Type, c.Value)).ToArray();
//                var claimResult = userMgr.AddClaimsAsync(user, claimList).Result;
//                if (!claimResult.Succeeded)
//                    throw new Exception(claimResult.Errors.First().Description);

//                Log.Debug($"{userName} created");
//            }
//            else
//            {
//                // update properties if missing
//                var mustUpdate = false;
//                if (user.GivenName != givenName)
//                {
//                    user.GivenName = givenName;
//                    mustUpdate = true;
//                }
//                if (user.FamilyName != familyName)
//                {
//                    user.FamilyName = familyName;
//                    mustUpdate = true;
//                }
//                if (mustUpdate)
//                {
//                    var updateResult = userMgr.UpdateAsync(user).Result;
//                    if (!updateResult.Succeeded)
//                        throw new Exception(updateResult.Errors.First().Description);
//                }

//                Log.Debug($"{userName} already exists");
//            }
//        }
//    }
//}
using Duende.IdentityModel;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace IdentityServerAspNetIdentity
{
    public static class SeedData
    {
        public static async Task EnsureSeedData(WebApplication app)
        {
            using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var sp = scope.ServiceProvider;

            // 1) Migrate ASP.NET Identity DB
            var db = sp.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();

            // 2) Migrate IdentityServer configuration DBs
            var configDb = sp.GetRequiredService<ConfigurationDbContext>();
            configDb.Database.Migrate();
            var persistedGrantDb = sp.GetRequiredService<PersistedGrantDbContext>();
            persistedGrantDb.Database.Migrate();

            // 3) Seed ASP.NET Identity roles and users
            var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var roleMgr = sp.GetRequiredService<RoleManager<ApplicationRole>>();

            string[] roles = { AppRoles.Admin, AppRoles.ProjectManager, AppRoles.Viewer };
            foreach (var r in roles)
            {
                var exists = await roleMgr.RoleExistsAsync(r);
                if (!exists)
                {
                    var roleResult = await roleMgr.CreateAsync(new ApplicationRole { Name = r });
                    if (!roleResult.Succeeded)
                        throw new Exception(roleResult.Errors.First().Description);
                }
            }

            // Add claims to roles
            await AddRoleClaims(roleMgr, AppRoles.Admin, new[] {
                AppPermissions.ListRoles, AppPermissions.CreateRole, AppPermissions.AssignRole
            });
            await AddRoleClaims(roleMgr, AppRoles.ProjectManager, new[] {
                AppPermissions.ListRoles, AppPermissions.AssignRole
            });
            await AddRoleClaims(roleMgr, AppRoles.Viewer, new[] {
                AppPermissions.ListRoles
            });

            // Seed users
            await SeedUser(userMgr, "alice", "AliceSmith@example.com", "Pass123$", "red", "Alice", "Smith",
                (JwtClaimTypes.GivenName, "Alice"),
                (JwtClaimTypes.FamilyName, "Smith"),
                (JwtClaimTypes.WebSite, "http://alice.example.com"),
                ("location", "WGT CA office")
            );
            await SeedUser(userMgr, "bob", "BobSmith@example.com", "Pass123$", "blue", "Bob", "Smith",
                (JwtClaimTypes.GivenName, "Bob"),
                (JwtClaimTypes.FamilyName, "Smith"),
                (JwtClaimTypes.WebSite, "http://bob.example.com"),
                ("location", "WGT CA office")
            );
            await SeedUser(userMgr, "tom", "tomsmith@example.com", "Pass123$", "green", "Tom", "Smith",
                (JwtClaimTypes.GivenName, "Tom"),
                (JwtClaimTypes.FamilyName, "Smith"),
                (JwtClaimTypes.WebSite, "http://tom.example.com"),
                ("location", "WGT SA office")
            );
            await SeedUser(userMgr, "john", "johnsmith@example.com", "Pass123$", "purple", "John", "Smith",
                (JwtClaimTypes.GivenName, "John"),
                (JwtClaimTypes.FamilyName, "Smith"),
                (JwtClaimTypes.WebSite, "http://john.example.com"),
                ("location", "WGT CA office")
            );

            // Assign Alice to Admin role
            var alice = await userMgr.FindByNameAsync("alice");
            if (!await userMgr.IsInRoleAsync(alice, AppRoles.Admin))
            {
                var addRoleResult = await userMgr.AddToRoleAsync(alice, AppRoles.Admin);
                if (!addRoleResult.Succeeded)
                    throw new Exception(addRoleResult.Errors.First().Description);
            }

            // 4) Seed IdentityServer configuration data
            await EnsureIdentityServerSeedData(configDb);

            Log.Debug("Seeding complete: roles, users, and IdentityServer configuration created.");
        }

        private static async Task AddRoleClaims(RoleManager<ApplicationRole> roleMgr, string roleName, string[] permissions)
        {
            var role = await roleMgr.FindByNameAsync(roleName);
            if (role != null)
            {
                var existingClaims = await roleMgr.GetClaimsAsync(role);
                foreach (var permission in permissions)
                {
                    if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                    {
                        await roleMgr.AddClaimAsync(role, new Claim("permission", permission));
                    }
                }
            }
        }

        private static async Task SeedUser(
            UserManager<ApplicationUser> userMgr,
            string userName,
            string email,
            string password,
            string favoriteColor,
            string givenName,
            string familyName,
            params (string Type, string Value)[] extraClaims)
        {
            var user = await userMgr.FindByNameAsync(userName);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true,
                    FavoriteColor = favoriteColor,
                    GivenName = givenName,
                    FamilyName = familyName
                };
                var createResult = await userMgr.CreateAsync(user, password);
                if (!createResult.Succeeded)
                    throw new Exception(createResult.Errors.First().Description);

                var claimList = extraClaims.Select(c => new Claim(c.Type, c.Value)).ToArray();
                var claimResult = await userMgr.AddClaimsAsync(user, claimList);
                if (!claimResult.Succeeded)
                    throw new Exception(claimResult.Errors.First().Description);

                Log.Debug($"{userName} created");
            }
            else
            {
                // update properties if missing
                var mustUpdate = false;
                if (user.GivenName != givenName)
                {
                    user.GivenName = givenName;
                    mustUpdate = true;
                }
                if (user.FamilyName != familyName)
                {
                    user.FamilyName = familyName;
                    mustUpdate = true;
                }
                if (mustUpdate)
                {
                    var updateResult = await userMgr.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                        throw new Exception(updateResult.Errors.First().Description);
                }

                Log.Debug($"{userName} already exists");
            }
        }

        // IdentityServer configuration seeding
        public static async Task EnsureIdentityServerSeedData(ConfigurationDbContext context)
        {
            // Seed Clients
            if (!context.Clients.Any())
            {
                foreach (var client in Config.Clients)
                {
                    context.Clients.Add(client.ToEntity());
                }
                await context.SaveChangesAsync();
            }

            // Seed IdentityResources
            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.IdentityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                await context.SaveChangesAsync();
            }

            // Seed ApiScopes
            if (!context.ApiScopes.Any())
            {
                foreach (var apiScope in Config.ApiScopes)
                {
                    context.ApiScopes.Add(apiScope.ToEntity());
                }
                await context.SaveChangesAsync();
            }

            // Seed ApiResources (if you have any)
            if (Config.ApiResources != null && !context.ApiResources.Any())
            {
                foreach (var apiResource in Config.ApiResources)
                {
                    context.ApiResources.Add(apiResource.ToEntity());
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
