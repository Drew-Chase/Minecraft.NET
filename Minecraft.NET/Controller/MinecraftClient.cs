// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Model;
using Chase.Networking;
using Newtonsoft.Json;

namespace Chase.Minecraft.Controller;

public class MinecraftClient : IDisposable
{
    private readonly NetworkClient _client;
    private readonly ClientStartInfo _clientStartInfo;

    public MinecraftClient(ClientStartInfo clientStartInfo)
    {
        _client = new NetworkClient();
        _clientStartInfo = clientStartInfo;
    }

    public async Task DownloadAllLibs()
    {
        string url = "https://piston-meta.mojang.com/v1/packages/715ccf3330885e75b205124f09f8712542cbe7e0/1.20.1.json";
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(json);
                    foreach (var library in data.libraries)
                    {
                        string libUrl = library.url;
                        string libPath = library.path;

                        Console.WriteLine("URL: " + libUrl);
                        Console.WriteLine("Path: " + libPath);
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine("Request failed with status code: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}