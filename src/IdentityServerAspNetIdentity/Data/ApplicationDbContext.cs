using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, int>(options)
{
    protected override void OnModelCreating(ModelBuilder builder) => base.OnModelCreating(builder);
}
