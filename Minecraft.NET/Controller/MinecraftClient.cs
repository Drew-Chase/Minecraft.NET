// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Model;
using Chase.Networking;
using Newtonsoft.Json.Linq;

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

        HttpResponseMessage response = await _client.GetAsync("https://piston-meta.mojang.com/v1/packages/715ccf3330885e75b205124f09f8712542cbe7e0/1.20.1.json");

        if (response.IsSuccessStatusCode)
        {
            DownloadArtifact[] artifacts = JObject.Parse(await response.Content.ReadAsStringAsync())["libraries"]?.ToObject<DownloadArtifact[]>() ?? Array.Empty<DownloadArtifact>();
            foreach (DownloadArtifact artifact in artifacts)
            {
                string absolutePath = Path.Combine(libBasePath, artifact.Downloads.Artifact.Path);
                string filename = absolutePath.Split('/').Last();
                await Console.Out.WriteLineAsync($"Downloading '{artifact.Downloads.Artifact.Path}'");
                string directory = Directory.CreateDirectory(Directory.GetParent(absolutePath)?.FullName ?? "").FullName;
                await _client.DownloadFileAsync(new Uri(artifact.Downloads.Artifact.Url), absolutePath, (s, e) =>
                {
                    Console.WriteLine(e.Percentage.ToString("P2"));
                });
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