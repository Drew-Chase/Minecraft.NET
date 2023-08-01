/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Exceptions;
using Chase.Minecraft.Model;
using Chase.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Chase.Minecraft.Authentication;

/// <summary>
/// Provides methods for authenticating with Microsoft services to obtain a Minecraft bearer access token.
/// </summary>
public static class MicrosoftAuthentication
{
    /// <summary>
    /// Retrieves a Minecraft bearer access token by authenticating with Microsoft services.
    /// </summary>
    /// <param name="clientId">The client ID used for authentication.</param>
    /// <param name="redirectUri">The redirect URI used for authentication.</param>
    /// <param name="authenticationFile">
    /// The file to store authentication information (optional, defaults to "msa-auth.json").
    /// </param>
    /// <param name="onlyRefresh">This will only attempt to refresh the pre-existing login.</param>
    /// <returns>
    /// The Minecraft bearer access token if authentication is successful; otherwise, null.
    /// </returns>
    public static async Task<string?> GetMinecraftBearerAccessToken(string clientId, string redirectUri, string authenticationFile = "msa-auth.json", bool onlyRefresh = false)
    {
        if (!File.Exists(authenticationFile) && onlyRefresh)
        {
            return null;
        }
        try
        {
            using NetworkClient client = new();
            XboxLiveAuthResponse? xboxLiveAuthResponse = await GetXboxLiveAuthResponseAsync(client, authenticationFile, clientId, redirectUri, onlyRefresh);
            if (xboxLiveAuthResponse == null) { return null; }

            string? xsts = await GetXSTSToken(client, xboxLiveAuthResponse);
            if (xsts != null)
            {
                string json = JsonConvert.SerializeObject(new
                {
                    identityToken = $"XBL3.0 x={xboxLiveAuthResponse.Value.DisplayClaims.XUI.First().UHS};{xsts}",
                    ensureLegacyEnabled = true
                });
                using HttpRequestMessage request = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new("https://api.minecraftservices.com/authentication/login_with_xbox"),
                    Content = new StringContent(json),
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using HttpResponseMessage response = await client.SendAsync(request);
                string content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return JObject.Parse(content)["access_token"]?.ToObject<string>();
                }
                else
                {
                    throw new MinecraftBearerException(xsts, content);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to Get Minecraft Bearer Token: Client ID: {CID}, Redirect URI: {URI}", clientId, redirectUri, ex);
        }

        return null;
    }

    /// <summary>
    /// Gets the user profile from the minecraft api. See: <seealso
    /// cref="GetMinecraftBearerAccessToken(string, string, string)">GetMinecraftBearerAccessToken</seealso>
    /// </summary>
    /// <param name="accessToken">
    /// The minecraft authentication bearer token <seealso
    /// cref="GetMinecraftBearerAccessToken(string, string, string)">GetMinecraftBearerAccessToken</seealso>
    /// </param>
    /// <returns></returns>
    public static async Task<UserProfile?> GetUserProfile(string accessToken)
    {
        try
        {
            using NetworkClient client = new();
            using HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new("https://api.minecraftservices.com/minecraft/profile"),
            };
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            return (await client.GetAsJson(request))?.ToObject<UserProfile>();
        }
        catch (Exception e)
        {
            Log.Error("Failed to fetch the users profile: Access Token: {TOKEN}", accessToken, e);
            return null;
        }
    }

    private static string GenerateCodeVerifier()
    {
        const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~";
        var random = new Random();
        var codeVerifier = new StringBuilder(128);

        for (int i = 0; i < 128; i++)
        {
            int randomIndex = random.Next(allowedChars.Length);
            char randomChar = allowedChars[randomIndex];
            codeVerifier.Append(randomChar);
        }

        return codeVerifier.ToString();
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        byte[] codeVerifierBytes = Encoding.ASCII.GetBytes(codeVerifier);
        byte[] codeChallengeBytes = sha256.ComputeHash(codeVerifierBytes);
        string codeChallenge = Convert.ToBase64String(codeChallengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return codeChallenge;
    }

    private static async Task<string?> GetXSTSToken(NetworkClient client, XboxLiveAuthResponse? xboxLiveAuth)
    {
        try
        {
            if (xboxLiveAuth != null)
            {
                using HttpRequestMessage request = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new("https://xsts.auth.xboxlive.com/xsts/authorize"),
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        Properties = new
                        {
                            SandboxId = "RETAIL",
                            UserTokens = new string[]
                            {
                            xboxLiveAuth.Value.Token
                            }
                        },
                        RelyingParty = "rp://api.minecraftservices.com/",
                        TokenType = "JWT"
                    }))
                };
                using HttpResponseMessage response = await client.SendAsync(request);
                string content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    JObject json = JObject.Parse(content);
                    return json["Token"]?.ToObject<string>();
                }
                else if (response != null)
                {
                    throw new XSTSException(xboxLiveAuth.Value, content);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to get XSTS Token!", ex);
        }
        return null;
    }

    private static async Task<XboxLiveAuthResponse?> GetXboxLiveAuthResponseAsync(NetworkClient client, string authenticationFile, string clientId, string redirectUri, bool onlyRefresh)
    {
        try
        {
            MicrosoftToken? microsoftToken = await GetMicrosoftAccessToken(client, authenticationFile, clientId, redirectUri, onlyRefresh);
            if (microsoftToken != null)
            {
                using HttpRequestMessage request = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"https://user.auth.xboxlive.com/user/authenticate"),
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        Properties = new
                        {
                            AuthMethod = "RPS",
                            SiteName = "user.auth.xboxlive.com",
                            RpsTicket = "d=" + microsoftToken.Value.AccessToken
                        },
                        RelyingParty = "http://auth.xboxlive.com",
                        TokenType = "JWT"
                    }))
                };
                request.Headers.Add("Accept", "application/json");
                using HttpResponseMessage response = await client.SendAsync(request);
                string content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    XboxLiveAuthResponse? authResponse = JObject.Parse(content).ToObject<XboxLiveAuthResponse>();
                    return authResponse;
                }
                else if (response != null)
                {
                    throw new XboxLiveAuthenticationException(microsoftToken.Value, content);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error("Failed to get the XBOX Live Authentication Response!", e);
        }

        return null;
    }

    private static async Task<MicrosoftToken?> GetMicrosoftAccessToken(NetworkClient client, string authenticationFile, string clientId, string redirectUri, bool onlyRefresh)
    {
        try
        {
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = GenerateCodeChallenge(codeVerifier);
            if (File.Exists(authenticationFile))
            {
                try
                {
                    MicrosoftToken? token = await RefreshMicrosoftAccessToken(client, authenticationFile, clientId, redirectUri);
                    if (token != null)
                    {
                        return token;
                    }
                }
                catch { }
            }
            if (onlyRefresh)
            {
                return null;
            }
            string? code = GetAuthenticationCodeFromBrowser(codeChallenge, clientId, redirectUri);
            if (code != null)
            {
                HttpRequestMessage request = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://login.live.com/oauth20_token.srf"),
                    Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                    {
                    new("client_id",clientId),
                    new("code",code),
                    new("code_verifier", codeVerifier),
                    new("grant_type","authorization_code"),
                    new("redirect_uri",redirectUri),
                    })
                };
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                using HttpResponseMessage response = await client.SendAsync(request);
                string content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    File.WriteAllText(authenticationFile, content);
                    return JObject.Parse(content).ToObject<MicrosoftToken>();
                }
                else if (response != null)
                {
                    throw new MicrosoftAuthenticationException(clientId, code, content);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error("Failed to fetch the Microsoft Access Token!", e);
        }
        return null;
    }

    private static async Task<MicrosoftToken?> RefreshMicrosoftAccessToken(NetworkClient client, string authenticationFile, string clientId, string redirectUri)
    {
        try
        {
            MicrosoftToken token;
            using (StreamReader reader = File.OpenText(authenticationFile))
            {
                token = JObject.Parse(reader.ReadToEnd()).ToObject<MicrosoftToken>();
            }

            using HttpRequestMessage request = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://login.live.com/oauth20_token.srf"),
                Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new("client_id",clientId),
                    new("refresh_token",token.RefreshToken),
                    new("grant_type","refresh_token"),
                    new("redirect_uri",redirectUri),
                })
            };

            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
            using HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                File.WriteAllText(authenticationFile, content);
                return JObject.Parse(content).ToObject<MicrosoftToken>();
            }
        }
        catch (Exception e)
        {
            Log.Error("Failed to refresh the Microsoft Access Token!", e);
        }
        return null;
    }

    private static string? GetAuthenticationCodeFromBrowser(string challengeCode, string clientId, string redirectUri)
    {
        try
        {
            string url = $"https://login.live.com/oauth20_authorize.srf?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope=XboxLive.signin%20offline_access&state=NOT_NEEDED&cobrandid=8058f65d-ce06-4c30-9559-473c9275a65d&prompt=select_account&code_challenge={challengeCode}&code_challenge_method=S256";

            Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = url });

            HttpListener listener = new();
            listener.Prefixes.Add(redirectUri.Trim('/').Trim() + "/");
            listener.Start();
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;

            string? queryParams = request.Url?.Query;
            if (queryParams != null)
            {
                SendCloseTabResponse(context.Response);

                Dictionary<string, string> queryParamsCollection = HttpUtility.ParseQueryString(queryParams).AllKeys.ToDictionary(key => key ?? "", key => HttpUtility.ParseQueryString(queryParams)[key] ?? "") ?? new Dictionary<string, string>();
                string json = JsonConvert.SerializeObject(queryParamsCollection);
                listener.Stop();
                return queryParamsCollection["code"].ToString();
            }
        }
        catch (Exception e)
        {
            Log.Error("Failed to authentication code from browser!", e);
        }
        return null;
    }

    private static void SendCloseTabResponse(HttpListenerResponse response)
    {
        string html = "<html><style>:root{color-scheme:dark;font-family:\"Roboto\";}body{display:flex;justify-content:center;align-items:center;flex-direction:column;height:100vh;margin:0;}</style><body><h1>Successfully Linked!</h1><h3>You can now close this tab</h3></body></html>";

        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(html);
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";

        using (var outputStream = response.OutputStream)
        {
            outputStream.Write(buffer, 0, buffer.Length);
        }

        response.Close();
    }
}