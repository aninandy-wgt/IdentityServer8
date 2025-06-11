//// Copyright (c) Duende Software. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1) Configure authentication to use JWT Bearer tokens
builder.Services.AddAuthentication()                       // registers the authentication services
    .AddJwtBearer(options =>                              // adds the “Bearer” handler
    {
        // a) Where to validate tokens (your IdentityServer URL)
        options.Authority = "https://localhost:5001";

        // b) We’re opting out of checking the “aud” claim here
        //    (because by default the JWT’s audience is the client ID, not “api1”)
        options.TokenValidationParameters.ValidateAudience = false;
    });

// 2) Define an authorization policy requiring the “api1” scope/claim
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1");
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// 3) Protect your endpoint with that policy
app.MapGet("identity", (ClaimsPrincipal user) =>
        user.Claims.Select(c => new { c.Type, c.Value }))
    .RequireAuthorization("ApiScope");

app.Run();