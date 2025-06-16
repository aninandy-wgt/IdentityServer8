using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication().AddJwtBearer(static options => { options.Authority = "https://localhost:5005"; options.TokenValidationParameters.ValidateAudience = false; });

builder.Services.AddAuthorizationBuilder().AddPolicy("ApiScope", static policy => { policy.RequireAuthenticatedUser(); policy.RequireClaim("scope", "api1"); });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("identity", (ClaimsPrincipal user) => user.Claims.Select(c => new { c.Type, c.Value })).RequireAuthorization("ApiScope");

app.Run();