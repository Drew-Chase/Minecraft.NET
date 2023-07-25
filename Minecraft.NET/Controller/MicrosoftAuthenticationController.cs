// LFInteractive LLC. 2021-2024﻿
using Microsoft.Identity.Client;

namespace Chase.Minecraft.Controller;

public static class MicrosoftAuthenticationController
{
    private const string Authority = "https://login.microsoftonline.com/consumers";
    private static readonly IEnumerable<string> Scope = new[] { "XboxLive.signin" };
    private static readonly string redirectUrl = "http://127.0.0.1:56748/msal";

    public static async Task LogIn(string clientId)
    {
        IPublicClientApplication app = PublicClientApplicationBuilder.Create(clientId).WithAuthority(Authority).WithRedirectUri(redirectUrl).Build();

        try
        {
            string client_info = "", state = "", code = "";
            //HttpListener listener = new();
            //listener.Prefixes.Add(redirectUrl + "/");
            //listener.Start();
            //_ = Task.Run(() =>
            //{
            //    HttpListenerContext context = listener.GetContext();
            //    HttpListenerRequest request = context.Request;

            //    string? queryParams = request.Url?.Query;
            //    if (queryParams != null)
            //    {
            //        Dictionary<string, string> queryParamsCollection = HttpUtility.ParseQueryString(queryParams).AllKeys.ToDictionary(key => key ?? "", key => HttpUtility.ParseQueryString(queryParams)[key] ?? "") ?? new Dictionary<string, string>();
            //        string json = JsonConvert.SerializeObject(queryParamsCollection);
            //        Console.WriteLine(json);
            //        code = queryParamsCollection["code"].ToString();
            //        client_info = queryParamsCollection["client_info"].ToString();
            //        state = queryParamsCollection["state"].ToString();
            //    }
            //});
            await Console.Out.WriteLineAsync("fasdfdas");
            AuthenticationResult result = await app
                .AcquireTokenInteractive(Scope)
                .ExecuteAsync();
            await Console.Out.WriteLineAsync("tits");
            //listener.Stop();

            Console.WriteLine($"Successfully logged in as {result.Account.Username}");
            Console.WriteLine($"Access Token: {result.AccessToken}");
            Console.WriteLine($"Expires On: {result.ExpiresOn}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login failed. Reason: {ex.Message}");
        }
    }
}