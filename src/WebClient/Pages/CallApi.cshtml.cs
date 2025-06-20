using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace WebClient.Pages;

public class CallApiModel(IHttpClientFactory httpClientFactory) : PageModel
{
    public string Json = string.Empty;

    public async Task OnGet()
    {
        var client = httpClientFactory.CreateClient("apiClient");

        var content = await client.GetStringAsync("https://localhost:6001/identity");

        var parsed = JsonDocument.Parse(content);
        var formatted = JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true });

        Json = formatted;
    }
}