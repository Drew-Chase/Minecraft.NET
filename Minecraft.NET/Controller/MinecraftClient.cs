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

    public async Task DownloadAllLibs(Uri manifestUri)
    {
        string libBasePath = Path.Combine(_clientStartInfo.Directory, "libraries");

        HttpResponseMessage response = await _client.GetAsync(manifestUri);

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

    public async Task DownloadAllAssets(Uri manifestUri)
    {
        string libBasePath = Path.Combine(_clientStartInfo.Directory, "assets");
        string resourcesBaseUrl = "https://resources.download.minecraft.net/";

        HttpResponseMessage response = await _client.GetAsync(manifestUri);

        if (response.IsSuccessStatusCode)
        {
            JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
            JObject objects = json["objects"] as JObject;

            if (objects != null)
            {
                foreach (JProperty property in objects.Properties())
                {
                    string fileName = property.Name;
                    string hash = property.Value["hash"].ToString();
                    string subFolder = hash.Substring(0, 2);
                    string fileUrl = $"{resourcesBaseUrl}/{subFolder}/{hash}";

                    string absolutePath = Path.Combine(libBasePath, fileName);
                    string directory = Directory.CreateDirectory(Directory.GetParent(absolutePath)?.FullName ?? "").FullName;

                    await Console.Out.WriteLineAsync($"Downloading '{fileName}'");
                    await _client.DownloadFileAsync(new Uri(fileUrl), absolutePath, (s, e) =>
                    {
                        Console.WriteLine(e.Percentage.ToString("P2"));
                    });
                }
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