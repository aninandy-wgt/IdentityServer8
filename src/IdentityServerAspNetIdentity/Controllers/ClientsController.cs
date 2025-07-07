using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.Models;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(ConfigurationDbContext configurationDbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllClients()
    {
        try
        {
            var clients = await configurationDbContext.Clients.ToListAsync();
            return Ok(clients);
        }
        catch (Exception) { throw; }
    }

    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetClient(int clientId)
    {
        try
        {
            var client = await configurationDbContext.Clients.FindAsync(clientId);
            return client == null ? NotFound() : Ok(client);
        }
        catch (Exception) { throw; }
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromServices] ConfigurationDbContext configurationDbContext, [FromBody] AddClientDto addRequest)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(addRequest.ClientId) || string.IsNullOrWhiteSpace(addRequest.Secret)) return BadRequest("ClientId required.");

            if (await configurationDbContext.Clients.AnyAsync(c => c.ClientId == addRequest.ClientId)) return Ok($"Client '{addRequest.ClientId}' already exists.");

            var newClient = new Duende.IdentityServer.EntityFramework.Entities.Client
            {
                ClientId = addRequest.ClientId,
                ClientName = addRequest.ClientName,
                ClientSecrets = [new() { Value = addRequest.Secret.Sha256() }],
                AllowedGrantTypes = [new() { GrantType = GrantTypes.Code.First() }],
                AllowedScopes = addRequest.Scopes?.Select(s => new ClientScope { Scope = s }).ToList() ?? [],
                AllowOfflineAccess = true,
                RedirectUris = addRequest.RedirectUris?.Select(u => new ClientRedirectUri { RedirectUri = u }).ToList() ?? [],
                PostLogoutRedirectUris = addRequest.PostLogoutRedirectUris?.Select(u => new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = u }).ToList() ?? [],
            };

            configurationDbContext.Clients.Add(newClient);
            await configurationDbContext.SaveChangesAsync();
            return Ok(new { newClient.Id, newClient.ClientId, newClient.ClientName });
        }
        catch (Exception) { throw; }
    }

    [HttpDelete("{clientId}")]
    public async Task<IActionResult> DeleteClient(int clientId)
    {
        try
        {
            var client = await configurationDbContext.Clients.FindAsync(clientId);
            if (client == null) return NotFound();
            
            configurationDbContext.Clients.Remove(client);
            await configurationDbContext.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception) { throw; }
    }
}