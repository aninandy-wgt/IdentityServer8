using IdentityServerAspNetIdentity;
using IdentityServerAspNetIdentity.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    var app = builder
        .ConfigureServices()  
        .ConfigurePipeline(); 

    if (args.Contains("/seed"))
    {
        Log.Information("Seeding database...");
        await SeedData.EnsureSeedData(app);
        Log.Information("Done seeding database. Exiting.");
        return;
    }
 
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

