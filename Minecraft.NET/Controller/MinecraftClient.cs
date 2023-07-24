// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Model;
using Chase.Networking;
using Chase.Networking.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Chase.Minecraft.Controller;

public class MinecraftClient : IDisposable
{
    private readonly NetworkClient _client;
    private readonly ClientStartInfo _clientStartInfo;
    private string assets = "";
    private string assetIndex = "";
    private string libraries = "";
    private string clientJar = "";
    private string authToken = "0";
    private string instanceDirectory = "";
    private MinecraftVersion version;

    public MinecraftClient(ClientStartInfo clientStartInfo)
    {
        _client = new NetworkClient();
        _clientStartInfo = clientStartInfo;
        Directory.CreateDirectory(clientStartInfo.Directory);
    }

    public static Process Launch(ClientStartInfo startInfo, string version, DataReceivedEventHandler? outputRecieved = null)
    {
        using MinecraftClient client = new(startInfo);
        MinecraftVersion? minecraftVersion = client.GetVersionByName(version);
        if (minecraftVersion != null && minecraftVersion.HasValue)
        {
            client.SetMinecraftVersion(minecraftVersion.Value);
            return client.Start(outputRecieved);
        }
        throw new NullReferenceException($"No minecraft version could be found with id of \"{version}\"");
    }

    public static Process Launch(ClientStartInfo startInfo, MinecraftVersion version, DataReceivedEventHandler? outputRecieved = null)
    {
        using MinecraftClient client = new(startInfo);
        client.SetMinecraftVersion(version);

        return client.Start(outputRecieved);
    }

    public void SetMinecraftVersion(MinecraftVersion version)
    {
        this.version = version;
        instanceDirectory = Path.Combine(_clientStartInfo.Directory, "instances", string.IsNullOrWhiteSpace(_clientStartInfo.Name) ? version.ID : _clientStartInfo.Name);
    }

    public Process Start(DataReceivedEventHandler? outputRecieved = null)
    {
        if (!LoadFromCache())
        {
            Task.WaitAll(DownloadAssets(), DownloadLibraries(), DownloadClient());
        }
        Process process = new()
        {
            StartInfo = new()
            {
                FileName = _clientStartInfo.JavaExecutable,
                Arguments = BuildJavaCommand(),
                UseShellExecute = false,
                WorkingDirectory = instanceDirectory
            },
            EnableRaisingEvents = true,
        };

        outputRecieved ??= (s, e) => { };
        process.OutputDataReceived += outputRecieved;

        process.Start();
        return process;
    }

    public async Task<MinecraftVersion?> GetVersionByNameAsync(string name) => (await GetMinecraftVersionManifestAsync())?.Versions.First(i => i.ID == name);

    public MinecraftVersion? GetVersionByName(string name) => GetVersionByNameAsync(name).Result;

    public async Task<MinecraftVersionManifest?> GetMinecraftVersionManifestAsync()
    {
        using HttpResponseMessage response = await _client.GetAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json");
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

    public async Task DownloadLibraries()
    {
        libraries = Directory.CreateDirectory(Path.Combine(_clientStartInfo.Directory, "libraries")).FullName;

        using HttpResponseMessage response = await _client.GetAsync(version.URL);

        if (response.IsSuccessStatusCode)
        {
            DownloadArtifact[] artifacts = JObject.Parse(await response.Content.ReadAsStringAsync())["libraries"]?.ToObject<DownloadArtifact[]>() ?? Array.Empty<DownloadArtifact>();
            List<Task> tasks = new();
            foreach (DownloadArtifact artifact in artifacts)
            {
                string absolutePath = Path.Combine(libraries, artifact.Downloads.Artifact.Path);
                string filename = absolutePath.Split('/').Last();
                await Console.Out.WriteLineAsync($"Downloading '{artifact.Downloads.Artifact.Path}'");
                string directory = Directory.CreateDirectory(Directory.GetParent(absolutePath)?.FullName ?? "").FullName;
                tasks.Add(_client.DownloadFileAsync(new Uri(artifact.Downloads.Artifact.Url), absolutePath, (s, e) => { }));
            }
            Task.WaitAll(tasks.ToArray());
            SaveToCache();
        }
        else
        {
            Console.WriteLine("Request failed with status code: " + response.StatusCode);
        }
    }

    public async Task DownloadClient(DownloadProgressEvent? progressEvent = null)
    {
        string? url = null;

        using (HttpResponseMessage response = await _client.GetAsync(version.URL))
        {
            if (response.IsSuccessStatusCode)
            {
                url = JObject.Parse(await response.Content.ReadAsStringAsync())?["downloads"]?["client"]?["url"]?.ToObject<string>();
            }
        }

        if (url != null)
        {
            progressEvent ??= (s, e) => { };
            clientJar = Path.Combine(Directory.CreateDirectory(Path.Combine(_clientStartInfo.Directory, "versions", version.ID)).FullName, "client.jar");
            await _client.DownloadFileAsync(new(url), clientJar, progressEvent);
        }
        SaveToCache();
    }

    public bool LoadFromCache()
    {
        string cacheFile = Path.Combine(Directory.CreateDirectory(Path.Combine(_clientStartInfo.Directory, "versions", version.ID)).FullName, "cache.json");
        if (!File.Exists(cacheFile))
        {
            return false;
        }
        using FileStream fs = new(cacheFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader reader = new(fs);
        JObject json = JObject.Parse(reader.ReadToEnd());
        libraries = json["libraries"]?.ToObject<string>() ?? "";
        assets = json["assets"]?.ToObject<string>() ?? "";
        assetIndex = json["assetIndex"]?.ToObject<string>() ?? "";
        authToken = json["authToken"]?.ToObject<string>() ?? "";
        clientJar = json["clientJar"]?.ToObject<string>() ?? "";
        return true;
    }

    public async Task DownloadAssets()
    {
        string assetsBasePath = Directory.CreateDirectory(Path.Combine(_clientStartInfo.Directory, "assets")).FullName;
        string indexesPath = Directory.CreateDirectory(Path.Combine(assetsBasePath, "indexes")).FullName;
        string resourcesBaseUrl = "https://resources.download.minecraft.net/";

        string url = "";
        string index = "";
        using (HttpResponseMessage response = await _client.GetAsync(version.URL))
        {
            if (response.IsSuccessStatusCode)
            {
                JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
                url = json["assetIndex"]?["url"]?.ToObject<string>() ?? "";
                index = json["assetIndex"]?["id"]?.ToObject<string>() ?? "";
            }
        }
        if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(index))
        {
            assetIndex = index;
            assets = assetsBasePath;
            using HttpResponseMessage response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                using (FileStream fs = new(Path.Combine(indexesPath, $"{index}.json"), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using StreamWriter writer = new(fs);
                    writer.Write(content);
                }
                JObject json = JObject.Parse(content);
                if (json["objects"] is JObject objects)
                {
                    List<Task> tasks = new();
                    foreach (JProperty property in objects.Properties())
                    {
                        string fileName = property.Name;
                        string hash = property.Value["hash"]?.ToString() ?? "";
                        string subFolder = hash[..2];
                        string fileUrl = $"{resourcesBaseUrl}/{subFolder}/{hash}";

                        string absolutePath = Path.Combine(assets, "objects", subFolder, hash);
                        string directory = Directory.CreateDirectory(Directory.GetParent(absolutePath)?.FullName ?? "").FullName;

                        await Console.Out.WriteLineAsync($"Downloading '{fileName}'");
                        tasks.Add(_client.DownloadFileAsync(new Uri(fileUrl), absolutePath, (s, e) => { }));
                    }
                    Task.WaitAll(tasks.ToArray());
                    SaveToCache();
                }
            }
        }
    }

    public string BuildJavaCommand()
    {
        string cmd = "";
        string classPaths = string.Join(';', Directory.GetFiles(libraries, "*.jar", SearchOption.AllDirectories));
        try
        {
            string natives = Path.Combine(_clientStartInfo.Directory, "natives", version.ID);
            cmd = $"-Djava.library.path=\"{natives}\" -Djna.tmpdir=\"{natives}\" -Dorg.lwjgl.system.SharedLibraryExtractPath=\"{natives}\" -Dio.netty.native.workdir=\"{natives}\" -Dminecraft.launcher.brand=better-minecraft-launcher -Dminecraft.launcher.version=0.0.1 -cp \"{classPaths};{clientJar}\" net.minecraft.client.main.Main --username {_clientStartInfo.OfflineUsername} --version {version.ID} --gameDir \"{instanceDirectory}\" --assetsDir \"{assets}\" --assetIndex {assetIndex} --accessToken 0";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }
        return cmd;
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private void SaveToCache()
    {
        string cacheFile = Path.Combine(Directory.CreateDirectory(Path.Combine(_clientStartInfo.Directory, "versions", version.ID)).FullName, "cache.json");
        using FileStream fs = new(cacheFile, FileMode.Create, FileAccess.Write, FileShare.None);
        using StreamWriter writer = new(fs);
        writer.Write(JsonConvert.SerializeObject(new
        {
            libraries,
            assets,
            assetIndex,
            authToken,
            clientJar
        }));
    }
}