// LFInteractive LLC. 2021-2024﻿

using Microsoft.Identity.Client;

namespace Chase.Minecraft.Controller;

public static class MicrosoftAuthenticationController
{
    private const string Authority = "https://login.microsoftonline.com/consumers";
    private static readonly IEnumerable<string> Scope = new[] { "https://api.minecraftservices.com/.default" };

    public static async Task LogIn(string clientId)
    {
        var app = PublicClientApplicationBuilder.Create(clientId)
            .WithAuthority(Authority)
            .WithRedirectUri($"http://127.0.0.1:888/msa")
            .Build();

        try
        {
            AuthenticationResult result = await app
                .AcquireTokenInteractive(Scope)
                .WithPrompt(Prompt.Consent)
                .ExecuteAsync();

            Console.WriteLine($"Successfully logged in as {result.Account.Username}");
            Console.WriteLine($"Access Token: {result.AccessToken}");
            Console.WriteLine($"Expires On: {result.ExpiresOn}");
        }
        catch (MsalException ex)
        {
            Console.WriteLine($"Login failed. Reason: {ex.Message}");
        }
    }
}