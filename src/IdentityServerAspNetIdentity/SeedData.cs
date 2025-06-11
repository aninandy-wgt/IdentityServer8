using System.Security.Claims;
using Duende.IdentityModel;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServerAspNetIdentity;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //for PSQL12 manually drop DB each time property added, might not be needed for 17
            //var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            context.Database.EnsureDeleted();
            context.Database.Migrate();

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
                alice = new ApplicationUser
                {
                    UserName = "alice",
                    Email = "AliceSmith@example.com",
                    EmailConfirmed = true,
                    FavoriteColor = "red",
                };
                var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(alice, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Alice Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Alice"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://alice.example.com"),
                            new Claim("location", "WGT CA office")
                        }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("alice created");
            }
            else
            {
                Log.Debug("alice already exists");
            }

            var bob = userMgr.FindByNameAsync("bob").Result;
            if (bob == null)
            {
                bob = new ApplicationUser
                {
                    UserName = "bob",
                    Email = "BobSmith@example.com",
                    EmailConfirmed = true,
                    FavoriteColor = "blue",
                };
                var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(bob, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Bob Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Bob"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://bob.example.com"),
                            new Claim("location", "WGT CA office")
                        }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("bob created");
            }
            else
            {
                Log.Debug("bob already exists");
            }

            // Generalize: Add missing GivenName and FamilyName claims for all users
            var allUsers = userMgr.Users.ToList();
            foreach (var user in allUsers)
            {
                var claims = userMgr.GetClaimsAsync(user).Result;
                var claimsToAdd = new List<Claim>();

                if (!claims.Any(c => c.Type == JwtClaimTypes.GivenName) && !string.IsNullOrEmpty(user.GivenName))
                {
                    claimsToAdd.Add(new Claim(JwtClaimTypes.GivenName, user.GivenName));
                }
                if (!claims.Any(c => c.Type == JwtClaimTypes.FamilyName) && !string.IsNullOrEmpty(user.FamilyName))
                {
                    claimsToAdd.Add(new Claim(JwtClaimTypes.FamilyName, user.FamilyName));
                }

                if (claimsToAdd.Count > 0)
                {
                    var result = userMgr.AddClaimsAsync(user, claimsToAdd).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Log.Debug($"Added missing claims for user {user.UserName}");
                }
            }
        }
    }
}
