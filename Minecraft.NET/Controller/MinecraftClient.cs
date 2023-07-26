/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Authentication;
using Chase.Minecraft.Instances;
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
    private readonly string Username;
    private readonly InstanceManager manager;
    private readonly string rootDirectory;
    private InstanceModel instance;
    private ClientInfo _clientInfo;
    private bool isAuthenticated = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftClient"/> class.
    /// </summary>
    /// <param name="instance">Information required to start the Minecraft client.</param>
    public MinecraftClient(string username, string rootDirectory, InstanceManager manager, InstanceModel instance)
    {
        _client = new NetworkClient();
        _clientInfo = new();
        Username = username;
        this.instance = instance;
        this.manager = manager;
        this.rootDirectory = Directory.CreateDirectory(rootDirectory).FullName;
    }

    public MinecraftClient(string username, string rootDirectory, InstanceManager manager, InstanceModel instance, string clientId, string clientName, string clientVersion) : this(username, rootDirectory, manager, instance)
    {
        SetClientInfo(clientId, clientName, clientVersion);
    }

    /// <summary>
    /// Launches the Minecraft client with the specified version.
    /// </summary>
    /// <param name="instance">Information required to start the Minecraft client.</param>
    /// <param name="version">The Minecraft version to launch.</param>
    /// <param name="outputRecieved">
    /// An optional event handler to receive the output data from the process.
    /// </param>
    /// <returns>The <see cref="Process"/> representing the launched Minecraft client.</returns>
    public static Process Launch(string username, string rootDirectory, InstanceManager manager, InstanceModel instance, DataReceivedEventHandler? outputRecieved = null)
    {
        using MinecraftClient client = new(username, rootDirectory, manager, instance);
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

    public async Task<bool> AuthenticateUser()
    {
        if (!string.IsNullOrWhiteSpace(_clientInfo.ClientID))
        {
            using MicrosoftAuthentication auth = new(_clientInfo.ClientID);
            string? token = await auth.GetMinecraftBearerAccessToken();
            if (token != null)
            {
                _clientInfo.AuthenticationToken = token;
                isAuthenticated = true;
                return true;
            }
        }
        isAuthenticated = false;
        return false;
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
                FileName = instance.Java,
                Arguments = BuildJavaCommand(),
                UseShellExecute = true,
                WorkingDirectory = instance.Path
            },
            EnableRaisingEvents = true,
        };

        outputRecieved ??= (s, e) => { };
        process.OutputDataReceived += outputRecieved;

        process.Start();
        return process;
    }

    /// <summary>
    /// Asynchronously downloads the Minecraft libraries required for the specified version.
    /// </summary>
    /// <remarks>
    /// Libraries will be saved in a "libraries" directory under the specified ClientStartInfo's directory.
    /// </remarks>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    public async Task DownloadLibraries()
    {
        _clientInfo.Libraries = Directory.CreateDirectory(Path.Combine(rootDirectory, "libraries")).FullName;

        using HttpResponseMessage response = await _client.GetAsync(instance.MinecraftVersion.URL);

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

        using (HttpResponseMessage response = await _client.GetAsync(instance.MinecraftVersion.URL))
        {
            if (response.IsSuccessStatusCode)
            {
                url = JObject.Parse(await response.Content.ReadAsStringAsync())?["downloads"]?["client"]?["url"]?.ToObject<string>();
            }
        }

        if (url != null)
        {
            progressEvent ??= (s, e) => { };
            instance.ClientJar = Path.Combine(Directory.CreateDirectory(Path.Combine(rootDirectory, "versions", instance.MinecraftVersion.ID)).FullName, "client.jar");
            await _client.DownloadFileAsync(new(url), instance.ClientJar, progressEvent);
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
        string cacheFile = Path.Combine(Directory.CreateDirectory(Path.Combine(rootDirectory, "versions", instance.MinecraftVersion.ID)).FullName, "cache.json");
        if (File.Exists(cacheFile))
        {
            _clientInfo = JObject.Parse(File.ReadAllText(cacheFile)).ToObject<ClientInfo>() ?? _clientInfo;
        }
        return ValidateAssets() && ValidateLibraries();
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
        string assetsBasePath = Directory.CreateDirectory(Path.Combine(rootDirectory, "assets")).FullName;
        string indexesPath = Directory.CreateDirectory(Path.Combine(assetsBasePath, "indexes")).FullName;
        string resourcesBaseUrl = "https://resources.download.minecraft.net/";

        string url = "";
        string index = "";
        using (HttpResponseMessage response = await _client.GetAsync(instance.MinecraftVersion.URL))
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

    private bool ValidateLibraries()
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
        return false;
    }

    private bool ValidateAssets()
    {
        if (Directory.Exists(_clientInfo.Assets) && Directory.Exists(Path.Combine(_clientInfo.Assets, "objects")) && Directory.Exists(Path.Combine(_clientInfo.Assets, "indexes")) && File.Exists(Path.Combine(_clientInfo.Assets, "indexes", $"{_clientInfo.AssetIndex}.json")))
        {
            Dictionary<string, long> names = Directory.GetFiles(Path.Combine(_clientInfo.Assets, "objects"), "*", SearchOption.AllDirectories).ToDictionary(i => Path.GetFileName(i), i => new FileInfo(i).Length);
            JObject? json = JObject.Parse(File.ReadAllText(Path.Combine(_clientInfo.Assets, "indexes", $"{_clientInfo.AssetIndex}.json")))["objects"]?.ToObject<JObject>();
            if (json != null)
            {
                bool hasAllObjects = true;
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
                if (hasAllObjects)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Builds the Java command required to start the Minecraft client process.
    /// </summary>
    /// <returns>The Java command as a string.</returns>
    private string BuildJavaCommand()
    {
        string cmd = "";
        string classPaths = string.Join(';', Directory.GetFiles(_clientInfo.Libraries, "*.jar", SearchOption.AllDirectories));
        instance = manager.Load(instance.Id) ?? instance;
        try
        {
            string natives = Directory.CreateDirectory(Path.Combine(rootDirectory, "natives", instance.MinecraftVersion.ID)).FullName;
            cmd = $"-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump -Xss1M -Djava.library.path=\"{natives}\" -Djna.tmpdir=\"{natives}\" -Dorg.lwjgl.system.SharedLibraryExtractPath=\"{natives}\" -Dio.netty.native.workdir=\"{natives}\" -Dminecraft.launcher.brand={_clientInfo.ClientName} -Dminecraft.launcher.version={_clientInfo.ClientVersion} -cp \"{classPaths};{instance.ClientJar}\" -Dlog4j.configurationFile=\"D:\\Workspaces\\Visual Studio Workspace\\C#\\Class Library\\Minecraft.NET\\Test\\bin\\Debug\\net7.0\\minecraft\\client-1.12.xml\" net.minecraft.client.main.Main --username {Username} --version {instance.MinecraftVersion.ID} --gameDir \"{instance.Path}\" --assetsDir \"{_clientInfo.Assets}\" --assetIndex {_clientInfo.AssetIndex} --accessToken {_clientInfo.AuthenticationToken} --clientId {_clientInfo.ClientID}";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }
        return cmd;
    }

    private void SaveToCache()
    {
        string cacheFile = Path.Combine(Directory.CreateDirectory(Path.Combine(rootDirectory, "versions", instance.MinecraftVersion.ID)).FullName, "cache.json");
        using (FileStream fs = new(cacheFile, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            using StreamWriter writer = new(fs);
            writer.Write(JsonConvert.SerializeObject(_clientInfo));
        }
        manager.Save(instance.Id, instance);
    }
}