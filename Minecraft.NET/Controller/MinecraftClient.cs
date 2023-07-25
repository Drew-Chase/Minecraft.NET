// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Model;
using Chase.Networking;
using Chase.Networking.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Chase.Minecraft.Controller;

/// <summary>
/// Represents a Minecraft client used to launch and interact with Minecraft game instances.
/// </summary>
/// <remarks>LFInteractive LLC. 2021-2024</remarks>
public class MinecraftClient : IDisposable
{
    private readonly NetworkClient _client;
    private ClientInfo _clientInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftClient"/> class.
    /// </summary>
    /// <param name="clientStartInfo">Information required to start the Minecraft client.</param>
    public MinecraftClient(ClientStartInfo clientStartInfo)
    {
        _client = new NetworkClient();
        _clientInfo = new ClientInfo()
        {
            ClientStartInfo = clientStartInfo,
        };
        Directory.CreateDirectory(clientStartInfo.Directory);
    }

    /// <summary>
    /// Launches the Minecraft client with the specified version.
    /// </summary>
    /// <param name="startInfo">Information required to start the Minecraft client.</param>
    /// <param name="version">The Minecraft version to launch.</param>
    /// <param name="outputRecieved">
    /// An optional event handler to receive the output data from the process.
    /// </param>
    /// <returns>The <see cref="Process"/> representing the launched Minecraft client.</returns>
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

    /// <summary>
    /// Launches the Minecraft client with the specified MinecraftVersion object.
    /// </summary>
    /// <param name="startInfo">Information required to start the Minecraft client.</param>
    /// <param name="version">The MinecraftVersion object to launch.</param>
    /// <param name="outputRecieved">
    /// An optional event handler to receive the output data from the process.
    /// </param>
    /// <returns>The <see cref="Process"/> representing the launched Minecraft client.</returns>
    public static Process Launch(ClientStartInfo startInfo, MinecraftVersion version, DataReceivedEventHandler? outputRecieved = null)
    {
        using MinecraftClient client = new(startInfo);
        client.SetMinecraftVersion(version);

        return client.Start(outputRecieved);
    }

    /// <summary>
    /// Sets the clients id, name and version. This is used by minecraft for telemetry and
    /// authentication. <br/> If you don't have a xbox client id you can get it from the <a
    /// href="https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps">Azure Portal</a>
    /// </summary>
    /// <param name="clientId">The xbox client id.</param>
    /// <param name="clientName">The name of the client</param>
    /// <param name="clientVersion">the clients version</param>
    public void SetClientInfo(string clientId, string clientName, string clientVersion)
    {
        _clientInfo.ClientID = clientId;
        _clientInfo.ClientName = clientName;
        _clientInfo.ClientVersion = clientVersion;
    }

    public async Task AuthenticateUser()
    {
        await MicrosoftAuthenticationController.LogIn(_clientInfo.ClientID);
    }

    /// <summary>
    /// Sets the Minecraft version to be used for launching the client.
    /// </summary>
    /// <param name="version">The Minecraft version to set.</param>
    public void SetMinecraftVersion(MinecraftVersion version)
    {
        _clientInfo.Version = version;
        _clientInfo.InstanceDirectory = Path.Combine(_clientInfo.ClientStartInfo.Directory, "instances", string.IsNullOrWhiteSpace(_clientInfo.ClientStartInfo.Name) ? _clientInfo.Version.ID : _clientInfo.ClientStartInfo.Name);
    }

    /// <summary>
    /// Starts the Minecraft client process.
    /// </summary>
    /// <param name="outputRecieved">
    /// An optional event handler to receive the output data from the process.
    /// </param>
    /// <returns>The <see cref="Process"/> representing the started Minecraft client process.</returns>
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

    /// <summary>
    /// Asynchronously gets a MinecraftVersion object by its name.
    /// </summary>
    /// <param name="name">The name of the Minecraft version to retrieve.</param>
    /// <returns>
    /// A Task that represents the asynchronous operation. The task result contains the retrieved
    /// MinecraftVersion object, if found; otherwise, null.
    /// </returns>
    public async Task<MinecraftVersion?> GetVersionByNameAsync(string name) => (await GetMinecraftVersionManifestAsync())?.Versions.First(i => i.ID == name);

    /// <summary>
    /// Gets a MinecraftVersion object by its name.
    /// </summary>
    /// <param name="name">The name of the Minecraft version to retrieve.</param>
    /// <returns>The retrieved MinecraftVersion object, if found; otherwise, null.</returns>
    public MinecraftVersion? GetVersionByName(string name) => GetVersionByNameAsync(name).Result;

    /// <summary>
    /// Asynchronously gets the Minecraft version manifest.
    /// </summary>
    /// <returns>
    /// A Task that represents the asynchronous operation. The task result contains the retrieved
    /// MinecraftVersionManifest object, if successful; otherwise, null.
    /// </returns>
    public async Task<MinecraftVersionManifest?> GetMinecraftVersionManifestAsync()
    {
        using HttpResponseMessage response = await _client.GetAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json");
        if (response.IsSuccessStatusCode)
        {
            return JObject.Parse(await response.Content.ReadAsStringAsync())?.ToObject<MinecraftVersionManifest>();
        }

        return null;
    }

    /// <summary>
    /// Asynchronously gets the latest Minecraft version from the version manifest.
    /// </summary>
    /// <returns>
    /// A Task that represents the asynchronous operation. The task result contains the latest
    /// MinecraftVersion object, if successful; otherwise, null.
    /// </returns>
    public async Task<MinecraftVersion?> GetLatestMinecraftVersionAsync()
    {
        var manifest = await GetMinecraftVersionManifestAsync();
        if (manifest != null && manifest.HasValue)
        {
            return manifest.Value.Versions.FirstOrDefault(i => i.ID == manifest?.Latest.Release);
        }
        return null;
    }

    /// <summary>
    /// Gets the Minecraft version manifest.
    /// </summary>
    /// <returns>The retrieved MinecraftVersionManifest object, if successful; otherwise, null.</returns>
    public MinecraftVersionManifest? GetMinecraftVersionManifest() => GetMinecraftVersionManifestAsync().Result;

    /// <summary>
    /// Asynchronously downloads the Minecraft libraries required for the specified version.
    /// </summary>
    /// <remarks>
    /// Libraries will be saved in a "libraries" directory under the specified ClientStartInfo's directory.
    /// </remarks>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    public async Task DownloadLibraries()
    {
        _clientInfo.Libraries = Directory.CreateDirectory(Path.Combine(_clientInfo.ClientStartInfo.Directory, "libraries")).FullName;

        using HttpResponseMessage response = await _client.GetAsync(_clientInfo.Version.URL);

        if (response.IsSuccessStatusCode)
        {
            _clientInfo.LibraryFiles = JObject.Parse(await response.Content.ReadAsStringAsync())["libraries"]?.ToObject<DownloadArtifact[]>() ?? Array.Empty<DownloadArtifact>();
            List<Task> tasks = new();
            foreach (DownloadArtifact artifact in _clientInfo.LibraryFiles)
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

    /// <summary>
    /// Asynchronously downloads the Minecraft client JAR file for the specified version.
    /// </summary>
    /// <param name="progressEvent">
    /// An optional event handler to receive the download progress data.
    /// </param>
    /// <remarks>
    /// The client JAR will be saved in a "versions/{versionID}" directory under the specified
    /// ClientStartInfo's directory.
    /// </remarks>
    /// <returns>A Task that represents the asynchronous operation.</returns>

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

    /// <summary>
    /// Loads the Minecraft client information from the cache if available.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the client information was loaded from the cache; otherwise, <c>false</c>.
    /// </returns>
    public bool LoadFromCache()
    {
        string cacheFile = Path.Combine(Directory.CreateDirectory(Path.Combine(_clientInfo.ClientStartInfo.Directory, "versions", _clientInfo.Version.ID)).FullName, "cache.json");
        if (File.Exists(cacheFile))
        {
            using (FileStream fs = new(cacheFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using StreamReader reader = new(fs);
                _clientInfo = JObject.Parse(reader.ReadToEnd()).ToObject<ClientInfo>() ?? _clientInfo;
            }

            if (_clientInfo != null)
            {
                if (Directory.Exists(_clientInfo.Assets))
                {
                    if (Directory.Exists(Path.Combine(_clientInfo.Assets, "objects")))
                    {
                        if (Directory.Exists(Path.Combine(_clientInfo.Assets, "indexes")))
                        {
                            if (File.Exists(Path.Combine(_clientInfo.Assets, "indexes", $"{_clientInfo.AssetIndex}.json")))
                            {
                                bool hasAllObjects = true;
                                using (FileStream fs = new(Path.Combine(_clientInfo.Assets, "indexes", $"{_clientInfo.AssetIndex}.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    Dictionary<string, long> names = Directory.GetFiles(Path.Combine(_clientInfo.Assets, "objects"), "*", SearchOption.AllDirectories).ToDictionary(i => Path.GetFileName(i), i => new FileInfo(i).Length);
                                    using StreamReader reader = new(fs);
                                    JObject? json = JObject.Parse(reader.ReadToEnd())["objects"]?.ToObject<JObject>();
                                    if (json != null)
                                    {
                                        foreach (KeyValuePair<string, JToken?> obj in json)
                                        {
                                            string? hash = obj.Value?["hash"]?.ToObject<string>();
                                            long? size = obj.Value?["size"]?.ToObject<long>();
                                            if (hash != null && size != null)
                                            {
                                                if (!names.ContainsKey(hash))
                                                {
                                                    hasAllObjects = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (hasAllObjects)
                                {
                                    if (Directory.Exists(_clientInfo.Libraries))
                                    {
                                        if (_clientInfo.LibraryFiles.Any())
                                        {
                                            foreach (DownloadArtifact file in _clientInfo.LibraryFiles)
                                            {
                                                if (!File.Exists(Path.Combine(_clientInfo.Libraries, file.Downloads.Artifact.Path)))
                                                {
                                                    return false;
                                                }
                                            }
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Asynchronously downloads the Minecraft assets for the specified version.
    /// </summary>
    /// <remarks>
    /// Assets will be saved in an "assets" directory under the specified ClientStartInfo's directory.
    /// </remarks>
    /// <returns>A Task that represents the asynchronous operation.</returns>

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

    /// <summary>
    /// Disposes the Minecraft client and releases any resources associated with it.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
    }

    /// <summary>
    /// Builds the Java command required to start the Minecraft client process.
    /// </summary>
    /// <returns>The Java command as a string.</returns>
    private string BuildJavaCommand()
    {
        string cmd = "";
        string classPaths = string.Join(';', Directory.GetFiles(_clientInfo.Libraries, "*.jar", SearchOption.AllDirectories));
        try
        {
            string natives = Path.Combine(_clientInfo.ClientStartInfo.Directory, "natives", _clientInfo.Version.ID);
            cmd = $"-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump -Xss1M -Djava.library.path=\"{natives}\" -Djna.tmpdir=\"{natives}\" -Dorg.lwjgl.system.SharedLibraryExtractPath=\"{natives}\" -Dio.netty.native.workdir=\"{natives}\" -Dminecraft.launcher.brand={_clientInfo.ClientName} -Dminecraft.launcher.version={_clientInfo.ClientVersion} -cp \"{classPaths};{_clientInfo.ClientJar}\" net.minecraft.client.main.Main --username {_clientInfo.ClientStartInfo.Username} --version {_clientInfo.Version.ID} --gameDir \"{_clientInfo.InstanceDirectory}\" --assetsDir \"{_clientInfo.Assets}\" --assetIndex {_clientInfo.AssetIndex} --accessToken  --clientId {_clientInfo.ClientID}";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }
        return cmd;
    }

    private void SaveToCache()
    {
        string cacheFile = Path.Combine(Directory.CreateDirectory(Path.Combine(_clientInfo.ClientStartInfo.Directory, "versions", _clientInfo.Version.ID)).FullName, "cache.json");
        using FileStream fs = new(cacheFile, FileMode.Create, FileAccess.Write, FileShare.None);
        using StreamWriter writer = new(fs);
        writer.Write(JsonConvert.SerializeObject(_clientInfo));
    }
}