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
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Chase.Minecraft.Authentication;

public class MicrosoftAuthentication : IDisposable
{
    private const string redirectUri = "http://127.0.0.1:56748/msal";
    private readonly string clientId;
    private readonly string authenticationFile;
    private readonly NetworkClient client;

    public MicrosoftAuthentication(string clientId, string authenticationFile = "msa-auth.json")
    {
        client = new();
        this.clientId = clientId;
        this.authenticationFile = authenticationFile;
    }

    public void Dispose()
    {
        client.Dispose();
    }

    public async Task<string?> GetMinecraftBearerAccessToken()
    {
        XboxLiveAuthResponse? xboxLiveAuthResponse = await GetXboxLiveAuthResponseAsync();
        if (xboxLiveAuthResponse == null) { return null; }

        string? xsts = await GetXSTSToken(xboxLiveAuthResponse);
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

        return null;
    }

    private async Task<string?> GetXSTSToken(XboxLiveAuthResponse? xboxLiveAuth)
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
        return null;
    }

    private async Task<XboxLiveAuthResponse?> GetXboxLiveAuthResponseAsync()
    {
        MicrosoftToken? microsoftToken = await GetMicrosoftAccessToken();
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

        return null;
    }

    private async Task<MicrosoftToken?> GetMicrosoftAccessToken()
    {
        string codeVerifier = GenerateCodeVerifier();
        string codeChallenge = GenerateCodeChallenge(codeVerifier);
        if (File.Exists(authenticationFile))
        {
            try
            {
                MicrosoftToken? token = await RefreshMicrosoftAccessToken();
                if (token != null)
                {
                    return token;
                }
            }
            catch { }
        }
        string? code = GetAuthenticationCodeFromBrowser(codeChallenge);
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
        return null;
    }

    private async Task<MicrosoftToken?> RefreshMicrosoftAccessToken()
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
        return null;
    }

    private string? GetAuthenticationCodeFromBrowser(string challengeCode)
    {
        string url = $"https://login.live.com/oauth20_authorize.srf?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope=XboxLive.signin%20offline_access&state=NOT_NEEDED&cobrandid=8058f65d-ce06-4c30-9559-473c9275a65d&prompt=select_account&code_challenge={challengeCode}&code_challenge_method=S256";

        Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = url });
        HttpListener listener = new();
        listener.Prefixes.Add(redirectUri + "/");
        listener.Start();
        HttpListenerContext context = listener.GetContext();
        HttpListenerRequest request = context.Request;

        string? queryParams = request.Url?.Query;
        if (queryParams != null)
        {
            Dictionary<string, string> queryParamsCollection = HttpUtility.ParseQueryString(queryParams).AllKeys.ToDictionary(key => key ?? "", key => HttpUtility.ParseQueryString(queryParams)[key] ?? "") ?? new Dictionary<string, string>();
            string json = JsonConvert.SerializeObject(queryParamsCollection);
            listener.Stop();
            return queryParamsCollection["code"].ToString();
        }

        return null;
    }

    private string GenerateCodeVerifier()
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

    private string GenerateCodeChallenge(string codeVerifier)
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
}