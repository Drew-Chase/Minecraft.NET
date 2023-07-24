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
    private ClientInfo _clientInfo;

    public MinecraftClient(ClientStartInfo clientStartInfo)
    {
        _client = new NetworkClient();
        _clientInfo = new ClientInfo()
        {
            ClientStartInfo = clientStartInfo,
        };
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
        _clientInfo.Version = version;
        _clientInfo.InstanceDirectory = Path.Combine(_clientInfo.ClientStartInfo.Directory, "instances", string.IsNullOrWhiteSpace(_clientInfo.ClientStartInfo.Name) ? _clientInfo.Version.ID : _clientInfo.ClientStartInfo.Name);
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
                FileName = _clientInfo.ClientStartInfo.JavaExecutable,
                Arguments = BuildJavaCommand(),
                UseShellExecute = false,
                WorkingDirectory = _clientInfo.InstanceDirectory
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
        _clientInfo.Libraries = Directory.CreateDirectory(Path.Combine(_clientInfo.ClientStartInfo.Directory, "libraries")).FullName;

        using HttpResponseMessage response = await _client.GetAsync(_clientInfo.Version.URL);

        if (response.IsSuccessStatusCode)
        {
            DownloadArtifact[] artifacts = JObject.Parse(await response.Content.ReadAsStringAsync())["libraries"]?.ToObject<DownloadArtifact[]>() ?? Array.Empty<DownloadArtifact>();
            List<Task> tasks = new();
            foreach (DownloadArtifact artifact in artifacts)
            {
                string absolutePath = Path.Combine(_clientInfo.Libraries, artifact.Downloads.Artifact.Path);
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

        using (HttpResponseMessage response = await _client.GetAsync(_clientInfo.Version.URL))
        {
            if (response.IsSuccessStatusCode)
            {
                url = JObject.Parse(await response.Content.ReadAsStringAsync())?["downloads"]?["client"]?["url"]?.ToObject<string>();
            }
        }

        if (url != null)
        {
            progressEvent ??= (s, e) => { };
            _clientInfo.ClientJar = Path.Combine(Directory.CreateDirectory(Path.Combine(_clientInfo.ClientStartInfo.Directory, "versions", _clientInfo.Version.ID)).FullName, "client.jar");
            await _client.DownloadFileAsync(new(url), _clientInfo.ClientJar, progressEvent);
        }
        SaveToCache();
    }

    public bool LoadFromCache()
    {
        string cacheFile = Path.Combine(Directory.CreateDirectory(Path.Combine(_clientInfo.ClientStartInfo.Directory, "versions", _clientInfo.Version.ID)).FullName, "cache.json");
        if (!File.Exists(cacheFile))
        {
            return false;
        }
        using FileStream fs = new(cacheFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader reader = new(fs);
        _clientInfo = JObject.Parse(reader.ReadToEnd()).ToObject<ClientInfo>() ?? _clientInfo;
        return true;
    }

    public async Task DownloadAssets()
    {
        string assetsBasePath = Directory.CreateDirectory(Path.Combine(_clientInfo.ClientStartInfo.Directory, "assets")).FullName;
        string indexesPath = Directory.CreateDirectory(Path.Combine(assetsBasePath, "indexes")).FullName;
        string resourcesBaseUrl = "https://resources.download.minecraft.net/";

        string url = "";
        string index = "";
        using (HttpResponseMessage response = await _client.GetAsync(_clientInfo.Version.URL))
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
            _clientInfo.AssetIndex = index;
            _clientInfo.Assets = assetsBasePath;
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

                        string absolutePath = Path.Combine(_clientInfo.Assets, "objects", subFolder, hash);
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
        string classPaths = string.Join(';', Directory.GetFiles(_clientInfo.Libraries, "*.jar", SearchOption.AllDirectories));
        try
        {
            string natives = Path.Combine(_clientInfo.ClientStartInfo.Directory, "natives", _clientInfo.Version.ID);
            cmd = $"-Djava.library.path=\"{natives}\" -Djna.tmpdir=\"{natives}\" -Dorg.lwjgl.system.SharedLibraryExtractPath=\"{natives}\" -Dio.netty.native.workdir=\"{natives}\" -Dminecraft.launcher.brand=better-minecraft-launcher -Dminecraft.launcher.version=0.0.1 -cp \"{classPaths};{_clientInfo.ClientJar}\" net.minecraft.client.main.Main --username {_clientInfo.ClientStartInfo.Username} --version {_clientInfo.Version.ID} --gameDir \"{_clientInfo.InstanceDirectory}\" --assetsDir \"{_clientInfo.Assets}\" --assetIndex {_clientInfo.AssetIndex} --accessToken {_clientInfo.AuthenticationToken}";
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
        string cacheFile = Path.Combine(Directory.CreateDirectory(Path.Combine(_clientInfo.ClientStartInfo.Directory, "versions", _clientInfo.Version.ID)).FullName, "cache.json");
        using FileStream fs = new(cacheFile, FileMode.Create, FileAccess.Write, FileShare.None);
        using StreamWriter writer = new(fs);
        writer.Write(JsonConvert.SerializeObject(_clientInfo));
    }
}