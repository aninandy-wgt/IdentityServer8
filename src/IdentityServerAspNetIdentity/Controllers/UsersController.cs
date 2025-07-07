using Duende.IdentityModel;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IdentityServerAspNetIdentity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(UserManager<ApplicationUser> userMgr, RoleManager<ApplicationRole> roleMgr) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await userMgr.Users.ToListAsync();
            return Ok(users);
        }
        catch (Exception) { throw; }        
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser(string userId)
    {
        try
        {
            var user = await userMgr.FindByIdAsync(userId);
            return user == null ? NotFound() : Ok(user);
        }
        catch (Exception) { throw; }        
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] ApplicationUser user)
    {
        try
        {
            if (user == null) return BadRequest("User data is null.");

            if (await userMgr.FindByNameAsync(user.UserName) != null) return Conflict($"User '{user.UserName}' already exists.");

            var result = await userMgr.CreateAsync(user, "Pass123$");
            if (!result.Succeeded) return BadRequest(result.Errors);

            user.TwoFactorEnabled = false;

            var claims = new List<Claim>
        {
            new(JwtClaimTypes.GivenName, user.GivenName),
            new(JwtClaimTypes.FamilyName, user.FamilyName),
            new("favorite_color", user.FavoriteColor)
        };
            await userMgr.AddClaimsAsync(user, claims);

            return Ok(new { user.Id, user.UserName, user.Email, user.EmailConfirmed });

        }
        catch (Exception) { throw; }        
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        try
        {
            var user = await userMgr.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var result = await userMgr.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok($"{user.GivenName} deleted!");
        }
        catch (Exception) { throw; }        
    }

    [HttpGet("{userId}/roles")]
    public async Task<IActionResult> GetUserRoles(string userId)
    {
        try
        {
            var user = await userMgr.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var roles = await userMgr.GetRolesAsync(user);
            return Ok(roles);
        }
        catch (Exception) { throw; }        
    }

    [HttpPost("{userId}/roles")]
    public async Task<IActionResult> AddUserToRole(string userId, [FromBody] string roleName)
    {
        try
        {
            var user = await userMgr.FindByIdAsync(userId);
            if (user == null) return NotFound();
            if (!await roleMgr.RoleExistsAsync(roleName)) return NotFound($"Role '{roleName}' does not exist.");
            var result = await userMgr.AddToRoleAsync(user, roleName);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(new { user.Id, user.UserName, Role = roleName });
        }
        catch (Exception) { throw; }        
    }

    [HttpDelete("{userId}/roles/{roleName}")]
    public async Task<IActionResult> RemoveUserFromRole(string userId, string roleName)
    {
        try
        {
            var user = await userMgr.FindByIdAsync(userId);
            if (user == null) return NotFound();
            if (!await roleMgr.RoleExistsAsync(roleName)) return NotFound($"Role '{roleName}' does not exist.");
            var result = await userMgr.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok($"Removed {user.GivenName} from {roleName}!");
        }
        catch (Exception) { throw; }        
    }

    [HttpGet("{userId}/allowed-clients")]
    public async Task<IActionResult> GetAllowedClients(string userId)
    {
        try
        {
            var user = await userMgr.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var allowedClients = (await userMgr.GetClaimsAsync(user)).Where(c => c.Type == "allowed_client").Select(c => c.Value).ToList();

            return Ok(allowedClients);

        }
        catch (Exception) { throw; }        
    }

    [HttpPost("{userId}/allowed-clients/{clientId}")]
    public async Task<IActionResult> AddAllowedClient(string userId, string clientId)
    {
        try
        {
            var user = await userMgr.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var claim = new Claim("allowed_client", clientId);
            var result = await userMgr.AddClaimAsync(user, claim);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(new { user.Id, user.UserName, AllowedClient = clientId });
        }
        catch (Exception) { throw; }
    }

    [HttpDelete("{userId}/allowed-clients/{clientId}")]
    public async Task<IActionResult> RemoveAllowedClient(string userId, string clientId)
    {
        try
        {
            var user = await userMgr.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var claim = new Claim("allowed_client", clientId);
            var result = await userMgr.RemoveClaimAsync(user, claim);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok($"Removed {user.GivenName}'s access to {clientId}!");
        }
        catch (Exception) { throw; }        
    }
}