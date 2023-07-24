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

    public async Task<MinecraftVersionManifest?> GetMinecraftVersionManifestAsync()
    {
        HttpResponseMessage response = await _client.GetAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json");
        if (response.IsSuccessStatusCode)
        {
            return JObject.Parse(await response.Content.ReadAsStringAsync())?.ToObject<MinecraftVersionManifest>();
        }

        return null;
    }

    public async Task<MinecraftVersion?> GetLatestMinecraftVersionAsync()
    {
        var manifest = await GetMinecraftVersionManifestAsync();
        if (manifest != null && manifest.HasValue)
        {
            return manifest.Value.Versions.FirstOrDefault(i => i.ID == manifest?.Latest.Release);
        }
        return null;
    }

    public MinecraftVersionManifest? GetMinecraftVersionManifest() => GetMinecraftVersionManifestAsync().Result;

    public async Task DownloadLibraries(MinecraftVersion version)
    {
        string libBasePath = Path.Combine(_clientStartInfo.Directory, "libraries");

        HttpResponseMessage response = await _client.GetAsync(version.URL);

        if (response.IsSuccessStatusCode)
        {
            DownloadArtifact[] artifacts = JObject.Parse(await response.Content.ReadAsStringAsync())["libraries"]?.ToObject<DownloadArtifact[]>() ?? Array.Empty<DownloadArtifact>();
            List<Task> tasks = new();
            foreach (DownloadArtifact artifact in artifacts)
            {
                string absolutePath = Path.Combine(libBasePath, artifact.Downloads.Artifact.Path);
                string filename = absolutePath.Split('/').Last();
                await Console.Out.WriteLineAsync($"Downloading '{artifact.Downloads.Artifact.Path}'");
                string directory = Directory.CreateDirectory(Directory.GetParent(absolutePath)?.FullName ?? "").FullName;
                tasks.Add(_client.DownloadFileAsync(new Uri(artifact.Downloads.Artifact.Url), absolutePath, (s, e) => { }));
            }
            Task.WaitAll(tasks.ToArray());
        }
        else
        {
            Console.WriteLine("Request failed with status code: " + response.StatusCode);
        }
    }

    public async Task DownloadAssets(MinecraftVersion version)
    {
        string libBasePath = Path.Combine(_clientStartInfo.Directory, "assets");
        string resourcesBaseUrl = "https://resources.download.minecraft.net/";

        HttpResponseMessage response = await _client.GetAsync(version.URL);
        string url = "";
        if (response.IsSuccessStatusCode)
        {
            JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
            url = json["assetIndex"]?["url"]?.ToObject<string>() ?? "";
        }
        if (!string.IsNullOrWhiteSpace(url))
        {
            response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
                if (json["objects"] is JObject objects)
                {
                    List<Task> tasks = new();
                    foreach (JProperty property in objects.Properties())
                    {
                        string fileName = property.Name;
                        string hash = property.Value["hash"]?.ToString() ?? "";
                        string subFolder = hash.Substring(0, 2);
                        string fileUrl = $"{resourcesBaseUrl}/{subFolder}/{hash}";

                        string absolutePath = Path.Combine(libBasePath, fileName);
                        string directory = Directory.CreateDirectory(Directory.GetParent(absolutePath)?.FullName ?? "").FullName;

                        await Console.Out.WriteLineAsync($"Downloading '{fileName}'");
                        tasks.Add(_client.DownloadFileAsync(new Uri(fileUrl), absolutePath, (s, e) => { }));
                    }
                    Task.WaitAll(tasks.ToArray());
                }
            }
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}