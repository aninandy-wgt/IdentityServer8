namespace IdentityServerAspNetIdentity.Models;

public class AddClientDto
{
    public string ClientId { get; set; } = "";

    public string? ClientName { get; set; } = "";

    public string? Secret { get; set; } = "";

    public List<string> RedirectUris { get; set; } = [];

    public List<string> PostLogoutRedirectUris { get; set; } = [];

    public List<string> Scopes { get; set; } = [];
}
