using IdentityServerAspNetIdentity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.OpenApi.Models;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "IdentityServer Admin API",
            Version = "v1",
            Description = "CRUD endpoints for clients, users, roles, etc."
        });
    });

    builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}").Enrich.FromLogContext().ReadFrom.Configuration(ctx.Configuration));

    var app = builder.ConfigureServices().ConfigurePipeline();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "STS Admin API v1");
        c.RoutePrefix = "swagger";
    });

    if (args.Contains("/seed")) { Log.Information("Seeding database..."); await SeedData.EnsureSeedData(app); Log.Information("Done seeding database. Exiting."); return; }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException) { Log.Fatal(ex, "Unhandled exception"); }
finally { Log.Information("Shut down complete"); Log.CloseAndFlush(); }
