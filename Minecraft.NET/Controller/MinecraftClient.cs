﻿/*
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
using Serilog;
using System.Diagnostics;
using OperatingSystem = System.OperatingSystem;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftClient"/> class.
    /// </summary>
    /// <param name="username">The players username</param>
    /// <param name="rootDirectory">
    /// The directory where all assets and library directories will be created
    /// </param>
    /// <param name="instance">Information required to start the Minecraft client.</param>
    public MinecraftClient(string username, string rootDirectory, InstanceModel instance)
    {
        _client = new NetworkClient();
        _clientInfo = new();
        Username = username;
        this.instance = instance;
        this.manager = instance.InstanceManager;
        this.rootDirectory = Directory.CreateDirectory(rootDirectory).FullName;
    }

    /// <summary>
    /// Launches the Minecraft client with the specified version.
    /// </summary>
    /// <param name="username">the players username</param>
    /// <param name="rootDirectory">
    /// The directory where all assets and library directories will be created
    /// </param>
    /// <param name="instance">Information required to start the Minecraft client.</param>
    /// <param name="clientId">
    /// The azure client id from <a
    /// href="https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps">Azure Portal</a>
    /// </param>
    /// <param name="clientName">
    /// The azure client name from <a
    /// href="https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps">Azure Portal</a>
    /// </param>
    /// <param name="clientVersion">The client version</param>
    /// <returns>The <see cref="Process"/> representing the launched Minecraft client.</returns>
    public MinecraftClient(string username, string rootDirectory, InstanceModel instance, string clientId, string clientName, string clientVersion, Uri redirectUri) : this(username, rootDirectory, instance)
    {
        SetClientInfo(clientId, clientName, clientVersion, redirectUri);
    }

    /// <summary>
    /// Launches the Minecraft client with the specified version.
    /// </summary>
    /// <param name="username">the players username</param>
    /// <param name="rootDirectory">
    /// The directory where all assets and library directories will be created
    /// </param>
    /// <param name="instance">Information required to start the Minecraft client.</param>
    /// <param name="outputRecieved">
    /// An optional event handler to receive the output data from the process.
    /// </param>
    /// <returns>The <see cref="Process"/> representing the launched Minecraft client.</returns>
    public static Process Launch(string username, string rootDirectory, InstanceModel instance, DataReceivedEventHandler? outputRecieved = null)
    {
        using MinecraftClient client = new(username, rootDirectory, instance);
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
    /// <param name="redirectUri">The azure client redirect uri</param>
    public void SetClientInfo(string clientId, string clientName, string clientVersion, Uri redirectUri)
    {
        Log.Debug("Setting client info: ID: {ID} Name: {NAME} Version: {VERSION}", clientId, clientName, clientVersion);
        _clientInfo.ClientID = clientId;
        _clientInfo.ClientName = clientName;
        _clientInfo.ClientVersion = clientVersion;
        _clientInfo.ClientRedirectUri = redirectUri;
        SaveToCache();
    }

    /// <summary>
    /// This authenticates the user using the client's info provided by the <seealso
    /// cref="SetClientInfo(string, string, string)">SetClientInfo</seealso>
    /// </summary>
    /// <returns>If the user was successfully authenticated.</returns>
    public async Task<bool> AuthenticateUser(string authenticationFile = "msa-auth.json")
    {
        if (!string.IsNullOrWhiteSpace(_clientInfo.ClientID))
        {
            string? token = await MicrosoftAuthentication.GetMinecraftBearerAccessToken(_clientInfo.ClientID, _clientInfo.ClientRedirectUri.ToString(), authenticationFile);
            if (token != null)
            {
                Log.Debug("Authenticated user!");
                _clientInfo.AuthenticationToken = token;
                UserProfile? profile = await MicrosoftAuthentication.GetUserProfile(token);
                if (profile != null)
                {
                    _clientInfo.UUID = profile.Value.Id;
                }
                SaveToCache();
                return true;
            }
        }
        Log.Warning("Failed to authenticate user");
        return false;
    }

    /// <summary>
    /// Starts the Minecraft client process.
    /// </summary>
    /// <param name="outputReceived">
    /// An optional event handler to receive the output data from the process.
    /// </param>
    /// <returns>The <see cref="Process"/> representing the started Minecraft client process.</returns>
    public Process Start(DataReceivedEventHandler? outputReceived = null)
    {
        LoadFromCache();
        Task.WaitAll(DownloadAssets(), DownloadLibraries(), DownloadClient());
        Process process = new()
        {
            StartInfo = new()
            {
                FileName = instance.Java,
                Arguments = BuildJavaCommand(),
                UseShellExecute = false,
                WorkingDirectory = instance.Path,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
            EnableRaisingEvents = true,
        };

        Log.Debug("Starting client with arguments: {args}", process.StartInfo.Arguments);

        outputReceived ??= (s, e) => { };
        process.OutputDataReceived += outputReceived;
        process.ErrorDataReceived += outputReceived;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    /// <summary>
    /// Asynchronously downloads the Minecraft libraries required for the specified version.
    /// </summary>
    /// <remarks>
    /// Libraries will be saved in a "libraries" directory under the specified ClientStartInfo's directory.
    /// </remarks>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    public async Task DownloadLibraries(bool force = false)
    {
        try
        {
            _clientInfo.LibrariesPath = Directory.CreateDirectory(Path.Combine(rootDirectory, "libraries")).FullName;
            _clientInfo.LibraryFiles = (await _client.GetAsJson(instance.MinecraftVersion.URL.ToString()))?["libraries"]?.ToObject<DownloadArtifact[]>() ?? Array.Empty<DownloadArtifact>();
            SaveToCache();
            if (ValidateLibraries() && !force)
            {
                return;
            }
            List<Task> tasks = new();
            List<string> paths = new();
            foreach (DownloadArtifact artifact in _clientInfo.LibraryFiles)
            {
                bool ok = true;
                try
                {
                    if (artifact.Rules != null && artifact.Rules.Any())
                    {
                        foreach (Rule rule in artifact.Rules)
                        {
                            if (!OperatingSystem.IsOSPlatform(rule.OS.Name))
                            {
                                ok = false;
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Warning("Unable to determine rule for artifact: {MSG}", e.Message, e);
                }
                if (ok)
                {
                    try
                    {
                        string absolutePath = Path.GetFullPath(Path.Combine(_clientInfo.LibrariesPath, artifact.Downloads.Artifact.Path));
                        string filename = Path.GetFileName(absolutePath);
                        Log.Debug($"[Libraries] Downloading '{artifact.Downloads.Artifact.Path}'");
                        paths.Add(absolutePath);
                        string directory = Directory.CreateDirectory(Directory.GetParent(absolutePath)?.FullName ?? "").FullName;
                        tasks.Add(_client.DownloadFileAsync(new Uri(artifact.Downloads.Artifact.Url), absolutePath, (s, e) => { }));
                    }
                    catch (Exception e)
                    {
                        Log.Error("Unable to download library file: {MSG}", e.Message, e);
                    }
                }
            }
            Task.WaitAll(tasks.ToArray());
            instance.ClassPaths = paths.ToArray();
            instance = instance.InstanceManager.Save(instance.Id, instance);
            Log.Debug("Libraries Download Completed!");
        }
        catch (Exception ex)
        {
            Log.Error("Unable to download library files: {MSG}", ex.Message, ex);
        }
    }

    /// <summary>
    /// Asynchronously downloads the Minecraft client JAR file for the specified version.
    /// </summary>
    /// <param name="force">If the client should be force downloaded</param>
    /// <param name="progressEvent">
    /// An optional event handler to receive the download progress data.
    /// </param>
    /// <remarks>
    /// The client JAR will be saved in a "versions/{versionID}" directory under the specified
    /// ClientStartInfo's directory.
    /// </remarks>
    /// <returns>A Task that represents the asynchronous operation.</returns>

    public async Task DownloadClient(bool force = false, DownloadProgressEvent? progressEvent = null)
    {
        if (ValidateClient() && !force)
        {
            return;
        }
        string? url = null;
        instance.GameVersion = instance.MinecraftVersion.ID;
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
            instance.ClientJar = Path.Combine(Directory.CreateDirectory(Path.Combine(rootDirectory, "versions", instance.MinecraftVersion.ID)).FullName, $"{instance.MinecraftVersion.ID}.jar");
            await _client.DownloadFileAsync(url, instance.ClientJar, progressEvent);
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
        return ValidateAssets() && ValidateLibraries() && ValidateClient();
    }

    /// <summary>
    /// Asynchronously downloads the Minecraft assets for the specified version.
    /// </summary>
    /// <remarks>
    /// Assets will be saved in an "assets" directory under the specified ClientStartInfo's directory.
    /// </remarks>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    public async Task DownloadAssets(bool force = false)
    {
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
        _clientInfo.Assets = Directory.CreateDirectory(Path.Combine(rootDirectory, "assets")).FullName;
        _clientInfo.AssetIndex = index;
        string indexesPath = Directory.CreateDirectory(Path.Combine(_clientInfo.Assets, "indexes")).FullName;
        if (ValidateAssets() && !force)
        {
            SaveToCache();
            return;
        }
        if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(index))
        {
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
                    List<string> items = new();
                    foreach (JProperty property in objects.Properties())
                    {
                        string fileName = property.Name;
                        string hash = property.Value["hash"]?.ToString() ?? "";
                        string subFolder = hash[..2];
                        string fileUrl = $"{resourcesBaseUrl}/{subFolder}/{hash}";

                        string absolutePath = Path.Combine(_clientInfo.Assets, "objects", subFolder, hash);
                        string directory = Directory.CreateDirectory(Directory.GetParent(absolutePath)?.FullName ?? "").FullName;

                        // Checks if the file has already been downloaded because Mojang has has
                        // duplicate assets in the assets directory for some reason!?!?!??!
                        if (!items.Contains(absolutePath))
                        {
                            Log.Debug($"[Assets] Downloading '{fileName}'");
                            tasks.Add(_client.DownloadFileAsync(new Uri(fileUrl), absolutePath, (s, e) => { }));
                            items.Add(absolutePath);
                        }
                    }
                    Task.WaitAll(tasks.ToArray());
                    Log.Debug("Asset Download Completed!");
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
        if (Directory.Exists(_clientInfo.LibrariesPath))
        {
            if (_clientInfo.LibraryFiles.Any())
            {
                foreach (DownloadArtifact file in _clientInfo.LibraryFiles)
                {
                    if (!File.Exists(Path.Combine(_clientInfo.LibrariesPath, file.Downloads.Artifact.Path)))
                    {
                        Log.Warning("Failed to validate libraries");
                        return false;
                    }
                }
                return true;
            }
        }
        Log.Warning("Failed to validate libraries");
        return false;
    }

    private bool ValidateClient()
    {
        string clientJar = Path.Combine(rootDirectory, "versions", instance.MinecraftVersion.ID, $"{instance.MinecraftVersion.ID}.jar");
        if (File.Exists(clientJar))
        {
            if (!File.Exists(instance.ClientJar))
            {
                instance.ClientJar = clientJar;
                instance.InstanceManager.Save(instance.Id, instance);
            }
            return true;
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
        Log.Warning("Failed to validate asset files");
        return false;
    }

    /// <summary>
    /// Builds the Java command required to start the Minecraft client process.
    /// </summary>
    /// <returns>The Java command as a string.</returns>
    private string BuildJavaCommand()
    {
        string cmd = "";
        string classPaths = string.Join(';', instance.ClassPaths);
        string gameVersion = string.IsNullOrWhiteSpace(instance.GameVersion) ? instance.MinecraftVersion.ID : instance.GameVersion;
        if (instance.ModLoader.Modloader == ModLoaders.Forge)
        {
            instance.ClientJar = Path.Combine(rootDirectory, "versions", $"{instance.MinecraftVersion.ID}-{instance.ModLoader.Version}", $"{instance.MinecraftVersion.ID}-{instance.ModLoader.Version}.jar");
            instance.InstanceManager.Save(instance.Id, instance);
        }
        try
        {
            string natives = Directory.CreateDirectory(Path.Combine(rootDirectory, "natives", instance.MinecraftVersion.ID)).FullName;

            string jvm =
                $"-Xmx{instance.RAM.Maximum}M " +
                $"-Xms{instance.RAM.Minimum}M " +
                $"-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump " +
                $"-Xss1M -Djava.library.path=\"{natives}\" -Djna.tmpdir=\"{natives}\" " +
                $"-Dorg.lwjgl.system.SharedLibraryExtractPath=\"{natives}\" " +
                $"-Dio.netty.native.workdir=\"{natives}\" " +
                $"-Dminecraft.launcher.brand=\"{_clientInfo.ClientName}\" " +
                $"-Dminecraft.launcher.version=\"{_clientInfo.ClientVersion}\" " +
                $"-cp \"{classPaths};{string.Join(";", instance.AdditionalClassPaths)};{instance.ClientJar}\" " +
                $"{string.Join(" ", instance.JVMArguments)}";

            string minecraftArgs = $"{string.Join(" ", instance.MinecraftArguments)} " +
                $"--uuid {_clientInfo.UUID} " +
                $"--username {Username} " +
                $"--version {gameVersion} " +
                $"--gameDir \"{instance.Path}\" " +
                $"--assetsDir \"{_clientInfo.Assets}\" " +
                $"--assetIndex {_clientInfo.AssetIndex} " +
                $"--accessToken {_clientInfo.AuthenticationToken} " +
                $"--clientId {_clientInfo.ClientID} " +
                $"--width {instance.WindowWidth} " +
                $"--height {instance.WindowHeight} --userType msa";

            cmd = $"{jvm} {instance.LaunchClassPath} {minecraftArgs}";
        }
        catch (Exception e)
        {
            Log.Error("Failed to build arguments", e);
        }
        return cmd;
    }

    private void SaveToCache()
    {
        Log.Debug("Saving client cache!");
        string cacheFile = Path.Combine(Directory.CreateDirectory(Path.Combine(rootDirectory, "versions", instance.MinecraftVersion.ID)).FullName, "cache.json");
        using (FileStream fs = new(cacheFile, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            using StreamWriter writer = new(fs);
            writer.Write(JsonConvert.SerializeObject(_clientInfo));
        }
        manager.Save(instance.Id, instance);
    }
}