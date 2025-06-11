// Copyright (c) Duende Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Duende.IdentityModel.Client;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MyApp.Namespace;

//QS3a:
//The httpClientFactory is injected using a primary constructor.
//The type was registered when AddOpenIdConnectAccessTokenManagement was called in Program.cs.
//The client is created using the factory passing in the name of the client
//that was registered in program.cs.
//No additional code is needed.The client will automatically retrieve the access token
//and refresh it if needed.
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


//public class CallApiModel : PageModel
//{
//    public string Json = string.Empty;

//    //QS3a:
//    //An object called tokenInfo containing all stored tokens is returned
//    //by the GetUserAccessTokenAsync extension method. This will make sure
//    //the access token is automatically refreshed using the refresh token if needed.
//    //The SetBearerToken extension method on HttpClient is used for convenience to
//    //place the access token in the needed HTTP header.
//    public async Task OnGet()
//    {
//        var tokenInfo = await HttpContext.GetUserAccessTokenAsync();
//        var client = new HttpClient();
//        client.SetBearerToken(tokenInfo.AccessToken!);

//        var content = await client.GetStringAsync("https://localhost:6001/identity");

//        var parsed = JsonDocument.Parse(content);
//        var formatted = JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true });

//        Json = formatted;
//    }

//    //public async Task OnGet()
//    //{
//    //    var accessToken = await HttpContext.GetTokenAsync("access_token");
//    //    var client = new HttpClient();
//    //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

//    //    var content = await client.GetStringAsync("https://localhost:6001/identity");

//    //    var parsed = JsonDocument.Parse(content);
//    //    var formatted = JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true });

//    //    Json = formatted;
//    //}
//}