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
        string libBasePath = Path.Combine(_clientStartInfo.Directory, "libraries");
        string url = "https://piston-meta.mojang.com/v1/packages/715ccf3330885e75b205124f09f8712542cbe7e0/1.20.1.json";

        HttpResponseMessage response = await _client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(json);
            foreach (var library in data.libraries)
            {
                string libUrl = library.downloads.artifact.url;
                string libPath = library.downloads.artifact.path;

                Console.WriteLine("URL: " + libUrl);
                Console.WriteLine("Path: " + libPath);


                //await _client.DownloadFileAsync(new Uri(libUrl), Directory.CreateDirectory(Path.Combine(libBasePath, Path.GetDirectoryName(libPath))).FullName, (s, e)=>{ });

                string directoryPath = Path.GetDirectoryName(libPath);
                Directory.CreateDirectory(directoryPath);

                await _client.DownloadFileAsync(new Uri(libUrl), libPath, (s, e) => { });

                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("Request failed with status code: " + response.StatusCode);
        }

    }

    public void Dispose()
    {
        _client.Dispose();
    }
}