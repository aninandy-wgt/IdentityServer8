using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController(ConfigurationDbContext configurationDbContext) : ControllerBase
{
    [HttpGet("identity-resources")]
    public async Task<IActionResult> GetAllIdentityResources()
    {
        try
        {
            var resources = await configurationDbContext.IdentityResources.ToListAsync();
            return Ok(resources);
        }
        catch (Exception) { throw; }        
    }

    [HttpGet("api-scopes")]
    public async Task<IActionResult> GetAllApiScopes()
    {
        try
        {
            var scopes = await configurationDbContext.ApiScopes.ToListAsync();
            return Ok(scopes);
        }
        catch (Exception) { throw; }        
    }

    [HttpGet("api-resources")]
    public async Task<IActionResult> GetAllApiResources()
    {
        try
        {
            var resources = await configurationDbContext.ApiResources.ToListAsync();
            return Ok(resources);
        }
        catch (Exception) { throw; }        
    }
}