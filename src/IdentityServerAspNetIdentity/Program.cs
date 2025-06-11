using IdentityServerAspNetIdentity;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Initialize a basic Serilog logger for early startup messages
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{

    // Create the application builder with default host settings
    var builder = WebApplication.CreateBuilder(args);

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

    //builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

    // Configure Serilog as the logging provider, reading settings from configuration
    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    // Register services and middleware via extension methods
    var app = builder
        .ConfigureServices()  // sets up EF Core, Identity, IdentityServer, Razor Pages, etc.
        .ConfigurePipeline(); // configures request pipeline: static files, routing, IdentityServer, auth, pages

    // Optional seed mode: populate the database and exit
    if (args.Contains("/seed"))
    {
        Log.Information("Seeding database...");
        SeedData.EnsureSeedData(app);
        Log.Information("Done seeding database. Exiting.");
        return; // exit after seeding
    }
 
    // Run the application and start listening for requests
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // Log any unexpected exceptions during startup
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    // Ensure all logs are flushed before exiting
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

