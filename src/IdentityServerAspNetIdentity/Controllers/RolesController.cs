using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IdentityServerAspNetIdentity.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController(RoleManager<ApplicationRole> roleManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            return Ok(await roleManager.Roles.Select(r => new { r.Id, r.Name }).ToListAsync());
        }
        catch (Exception) { throw; }        
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] string roleName)
    {
        try
        {
            if (await roleManager.RoleExistsAsync(roleName)) return Conflict();
            var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            return result.Succeeded ? Ok($"{roleName} created successfully!") : BadRequest(result.Errors);
        }
        catch (Exception) { throw; }        
    }

    [HttpDelete("{roleName}")]
    public async Task<IActionResult> DeleteRole(string roleName)
    {
        try
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) return NotFound();
            var result = await roleManager.DeleteAsync(role);
            return result.Succeeded ? Ok($"{roleName} deleted successfully!") : BadRequest(result.Errors);
        }
        catch (Exception) { throw; }        
    }

    [HttpGet("{roleName}/claims")]
    public async Task<IActionResult> GetRoleClaims(string roleName)
    {
        try
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) return NotFound();
            var claims = await roleManager.GetClaimsAsync(role);
            return Ok(claims.Select(c => new { c.Type, c.Value }));
        }
        catch (Exception) { throw; }        
    }

    [HttpPost("{roleName}/claims")]
    public async Task<IActionResult> AddRoleClaims(string roleName, [FromBody] List<AddRoleClaimsDto> dtos)
    {
        try
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) return NotFound();

            var toAdd = dtos.Where(dto => !string.IsNullOrEmpty(dto.Type) && !string.IsNullOrEmpty(dto.Value));
            foreach (var dto in toAdd)
            {
                if ((await roleManager.GetClaimsAsync(role)).Any(c => c.Type == dto.Type && c.Value == dto.Value))
                    continue;
                var result = await roleManager.AddClaimAsync(role, new Claim(dto.Type, dto.Value));
                if (!result.Succeeded) return BadRequest(result.Errors);
            }
            return Ok($"Claims added to {roleName}!");
        }
        catch (Exception) { throw; }        
    }

    [HttpDelete("{roleName}/claims")]
    public async Task<IActionResult> RemoveRoleClaims(string roleName, [FromBody] List<AddRoleClaimsDto> dtos)
    {
        try
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) return NotFound();

            var existing = await roleManager.GetClaimsAsync(role);
            var toRemove = existing.Where(c => dtos.Any(dto => dto.Type == c.Type && dto.Value == c.Value)).ToList();
            foreach (var claim in toRemove)
            {
                var result = await roleManager.RemoveClaimAsync(role, claim);
                if (!result.Succeeded) return BadRequest(result.Errors);
            }
            return Ok($"Claims deleted from {roleName}!");
        }
        catch (Exception) { throw; }        
    }
}
