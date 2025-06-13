using Duende.IdentityModel;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace IdentityServerAspNetIdentity//checking git sync
{
    public static class SeedData
    {
        public static async Task EnsureSeedData(WebApplication app)
        {
            using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var sp = scope.ServiceProvider;
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var roleMgr = sp.GetRequiredService<RoleManager<ApplicationRole>>();

            // 1) Migrate database
            db.Database.EnsureDeleted();
            db.Database.Migrate();

            // 2) Seed roles
            string[] roles = { AppRoles.Admin, AppRoles.ProjectManager, AppRoles.Viewer };
            foreach (var r in roles)
            {
                var exists = roleMgr.RoleExistsAsync(r).Result;
                if (!exists)
                {
                    var roleResult = roleMgr.CreateAsync(new ApplicationRole { Name = r }).Result;
                    if (!roleResult.Succeeded)
                        throw new Exception(roleResult.Errors.First().Description);
                }
            }

            // 2a) Add claims to roles
            var adminRole = await roleMgr.FindByNameAsync(AppRoles.Admin);
            if (adminRole != null)
            {
                var existingClaims = await roleMgr.GetClaimsAsync(adminRole);
                var permissions = new[]
                {
                    AppPermissions.ListRoles,
                    AppPermissions.CreateRole,
                    AppPermissions.AssignRole
                };
                foreach (var permission in permissions)
                {
                    if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                    {
                        await roleMgr.AddClaimAsync(adminRole, new Claim("permission", permission));
                    }
                }
            }

            var pmRole = await roleMgr.FindByNameAsync(AppRoles.ProjectManager);
            if (pmRole != null)
            {
                var existingClaims = await roleMgr.GetClaimsAsync(pmRole);
                var permissions = new[]
                {
                    AppPermissions.ListRoles,
                    AppPermissions.AssignRole
                };
                foreach (var permission in permissions)
                {
                    if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                    {
                        await roleMgr.AddClaimAsync(pmRole, new Claim("permission", permission));
                    }
                }
            }
            
            var viewerRole = await roleMgr.FindByNameAsync(AppRoles.Viewer);
            if (viewerRole != null) {
                var existingClaims = await roleMgr.GetClaimsAsync(viewerRole);
                var permissions = new[]
                {
                    AppPermissions.ListRoles
                };
                foreach (var permission in permissions)
                {
                    if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                    {
                        await roleMgr.AddClaimAsync(viewerRole, new Claim("permission", permission));
                    }
                }
            }

            // 3) Seed users and their properties/claims
            SeedUser(userMgr,
                userName: "alice",
                email: "AliceSmith@example.com",
                password: "Pass123$",
                favoriteColor: "red",
                givenName: "Alice",
                familyName: "Smith",
                (JwtClaimTypes.Name, "Alice Smith"), (JwtClaimTypes.GivenName, "Alice"),
    (JwtClaimTypes.FamilyName, "Smith"),
                (JwtClaimTypes.WebSite, "http://alice.example.com"),
                ("location", "WGT CA office")
            );

            SeedUser(userMgr,
                userName: "bob",
                email: "BobSmith@example.com",
                password: "Pass123$",
                favoriteColor: "blue",
                givenName: "Bob",
                familyName: "Smith", (JwtClaimTypes.GivenName, "Bob"),
    (JwtClaimTypes.FamilyName, "Smith"),
                (JwtClaimTypes.Name, "Bob Smith"),
                (JwtClaimTypes.WebSite, "http://bob.example.com"),
                ("location", "WGT CA office")
            );

            SeedUser(userMgr,
                userName: "tom",
                email: "tomsmith@example.com",
                password: "Pass123$",
                favoriteColor: "green",
                givenName: "Tom",
                familyName: "Smith", (JwtClaimTypes.GivenName, "Tom"),
    (JwtClaimTypes.FamilyName, "Smith"),
                (JwtClaimTypes.Name, "Tom Smith"),
                (JwtClaimTypes.WebSite, "http://tom.example.com"),
                ("location", "WGT SA office")
            );

            SeedUser(userMgr,
                userName: "john",
                email: "johnsmith@example.com",
                password: "Pass123$",
                favoriteColor: "purple",
                givenName: "John",
                familyName: "Smith", (JwtClaimTypes.GivenName, "John"),
    (JwtClaimTypes.FamilyName, "Smith"),
                (JwtClaimTypes.Name, "John Smith"),
                (JwtClaimTypes.WebSite, "http://john.example.com"),
                ("location", "WGT CA office")
            );

            // 4) Assign Alice to Admin role
            var alice = userMgr.FindByNameAsync("alice").Result;
            if (!userMgr.IsInRoleAsync(alice, AppRoles.Admin).Result)
            {
                var addRoleResult = userMgr.AddToRoleAsync(alice, AppRoles.Admin).Result;
                if (!addRoleResult.Succeeded)
                    throw new Exception(addRoleResult.Errors.First().Description);
            }

            Log.Debug("Seeding complete: roles created and users assigned.");
        }

        private static void SeedUser(
            UserManager<ApplicationUser> userMgr,
            string userName,
            string email,
            string password,
            string favoriteColor,
            string givenName,
            string familyName,
            params (string Type, string Value)[] extraClaims)
        {
            var user = userMgr.FindByNameAsync(userName).Result;
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
                var createResult = userMgr.CreateAsync(user, password).Result;
                if (!createResult.Succeeded)
                    throw new Exception(createResult.Errors.First().Description);

                var claimList = extraClaims.Select(c => new Claim(c.Type, c.Value)).ToArray();
                var claimResult = userMgr.AddClaimsAsync(user, claimList).Result;
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
                    var updateResult = userMgr.UpdateAsync(user).Result;
                    if (!updateResult.Succeeded)
                        throw new Exception(updateResult.Errors.First().Description);
                }

                Log.Debug($"{userName} already exists");
            }
        }
    }
}
