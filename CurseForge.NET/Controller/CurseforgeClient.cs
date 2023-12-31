﻿/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

// Ignore Spelling: Curseforge

using Chase.Minecraft.Curseforge.Model;
using Chase.Minecraft.Model;
using Chase.Networking;
using Chase.Networking.Event;
using Newtonsoft.Json.Linq;

namespace Chase.Minecraft.Curseforge.Controller;

/// <summary>
/// Represents a client for interacting with CurseForge's API to search for and retrieve
/// Minecraft-related content such as mods, modpacks, resource packs, and worlds.
/// </summary>
public class CurseforgeClient : IDisposable
{
    private static readonly string BASE_URI = "https://api.curseforge.com/v1/";
    private static readonly int GAME_ID = 432;
    private static readonly int MODS_SECTION_ID = 6;
    private static readonly int MODPACKS_SECTION_ID = 4471;
    private static readonly int RESOURCE_PACKS_SECTION_ID = 12;
    private static readonly int WORLDS_SECTION_ID = 17;
    private readonly NetworkClient _client;
    private readonly string _api;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurseforgeClient"/> class with the provided API key.
    /// </summary>
    /// <param name="api_key">The API key used to authenticate requests to the CurseForge API.</param>
    public CurseforgeClient(string api_key)
    {
        _client = new();
        _api = api_key;
    }

    public static string GetFileDirectDownloadUrl(CurseforgeProject project, ModFile mod)
    {
        if (!project.AllowModDistribution || mod.DownloadUrl == null)
        {
            string part1 = mod.Id.ToString()[..^3];
            string part2 = mod.Id.ToString()[^3..].TrimStart('0');
            return $"https://edge.forgecdn.net/files/{part1}/{part2}/{mod.FileName}";
        }
        return mod.DownloadUrl.ToString() ?? "";
    }

    /// <summary>
    /// Disposes of the resources used by the CurseforgeClient.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
    }

    /// <summary>
    /// Searches for mods that match the specified criteria asynchronously.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="gameVersion">The version of the Minecraft game.</param>
    /// <param name="loaders">The mod loaders to filter by.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the search results for mods.
    /// </returns>
    public Task<CurseforgeSearchResult?> SearchModsAsync(string query, string gameVersion, ModLoaders loaders) => SearchAsync(query, MODS_SECTION_ID, gameVersion, loaders);

    /// <summary>
    /// Searches for modpacks that match the specified criteria asynchronously.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="gameVersion">The version of the Minecraft game.</param>
    /// <param name="loaders">The mod loaders to filter by.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the search results for mods.
    /// </returns>
    public Task<CurseforgeSearchResult?> SearchModpackAsync(string query, string gameVersion, ModLoaders loaders) => SearchAsync(query, MODPACKS_SECTION_ID, gameVersion, loaders);

    /// <summary>
    /// Searches for worlds that match the specified criteria asynchronously.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="gameVersion">The version of the Minecraft game.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the search results for mods.
    /// </returns>
    public Task<CurseforgeSearchResult?> SearchWorldsAsync(string query, string gameVersion) => SearchAsync(query, WORLDS_SECTION_ID, gameVersion, null);

    /// <summary>
    /// Searches for resource packs that match the specified criteria asynchronously.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="gameVersion">The version of the Minecraft game.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the search results for mods.
    /// </returns>
    public Task<CurseforgeSearchResult?> SearchResourcepacksAsync(string query, string gameVersion) => SearchAsync(query, RESOURCE_PACKS_SECTION_ID, gameVersion, null);

    /// <summary>
    /// Retrieves detailed information about a specific mod asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the mod.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the detailed information
    /// about the mod.
    /// </returns>
    public Task<CurseforgeProject?> GetMod(string id) => GetProject(id, MODS_SECTION_ID);

    /// <summary>
    /// Retrieves detailed information about a specific modpack asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the modpack.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the detailed information
    /// about the modpack.
    /// </returns>
    public Task<CurseforgeProject?> GetModpack(string id) => GetProject(id, MODPACKS_SECTION_ID);

    /// <summary>
    /// Retrieves detailed information about a specific resource pack asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the resource pack.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the detailed information
    /// about the resource pack.
    /// </returns>
    public Task<CurseforgeProject?> GetResourcepack(string id) => GetProject(id, RESOURCE_PACKS_SECTION_ID);

    /// <summary>
    /// Retrieves detailed information about a specific world asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the world.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the detailed information
    /// about the world.
    /// </returns>
    public Task<CurseforgeProject?> GetWorld(string id) => GetProject(id, WORLDS_SECTION_ID);

    /// <summary>
    /// Downloads a mod file asynchronously.
    /// </summary>
    /// <param name="mod">The mod file to download.</param>
    /// <param name="instance">The instance model.</param>
    /// <param name="subfolder">The subfolder within the instance to save the downloaded file.</param>
    /// <param name="progressEvent">An optional event handler to track the download progress.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the path to the
    /// downloaded file.
    /// </returns>
    public Task<string> Download(CurseforgeProject project, ModFile mod, InstanceModel instance, string subfolder = "mods", DownloadProgressEvent? progressEvent = null) => Download(project, mod, Path.Combine(instance.Path, subfolder), progressEvent);

    /// <summary>
    /// Downloads a mod file asynchronously.
    /// </summary>
    /// <param name="mod">The mod file to download.</param>
    /// <param name="directory">the directory to download the file: example " <i>/path/to/mods</i>"</param>
    /// <param name="progressEvent">An optional event handler to track the download progress.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the path to the
    /// downloaded file.
    /// </returns>
    public async Task<string> Download(CurseforgeProject project, ModFile mod, string directory, DownloadProgressEvent? progressEvent = null)
    {
        string path = Path.Combine(directory, mod.FileName);
        await _client.DownloadFileAsync(GetFileDirectDownloadUrl(project, mod), path, progressEvent);
        return path;
    }

    /// <summary>
    /// Gets the projects page description as HTML.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<string?> GetProjectDescription(string id)
    {
        string url = $"{BASE_URI}mods/{id}/description";

        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Add("x-api-key", _api);
        return (await _client.GetAsJson(request))?["data"]?.ToObject<string>();
    }

    /// <summary>
    /// Gets the projects version changelog as HTML.
    /// </summary>
    /// <param name="project_id"></param>
    /// <param name="version_id"></param>
    /// <returns></returns>
    public async Task<string> GetProjectVersionDescription(string project_id, string version_id)
    {
        string url = $"{BASE_URI}mods/{project_id}/files/{version_id}/changelog";

        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Add("x-api-key", _api);
        JObject? json = await _client.GetAsJson(request);
        if (json != null)
        {
            if (json.ContainsKey("data"))
            {
                string cl = json["data"]?.ToObject<string>() ?? "";
                return cl;
            }
        }
        return "";
    }

    /// <summary>
    /// Retrieves project files associated with a specific project (mod, modpack, resource pack, or
    /// world) asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the project.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an array of project
    /// files associated with the project.
    /// </returns>
    public async Task<ModFile[]?> GetProjectFiles(string id)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"{BASE_URI}mods/{id}/files");
        request.Headers.Add("x-api-key", _api);
        return (await _client.GetAsJson(request))?["data"]?.ToObject<ModFile[]>();
    }

    /// <summary>
    /// Retrieves information about a specific project file associated with a project asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the project.</param>
    /// <param name="fileId">The unique identifier of the project file.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains detailed information
    /// about the project file.
    /// </returns>
    public async Task<ModFile?> GetProjectFile(string id, string fileId) => (await _client.GetAsJson($"{BASE_URI}mods/{id}/files/{fileId}"))?["data"]?.ToObject<ModFile>();

    /// <summary>
    /// Retrieves the latest project file for a specific project, optionally filtered by game
    /// version and loader.
    /// </summary>
    /// <param name="id">The unique identifier of the project.</param>
    /// <param name="gameVersion">
    /// Optional. The version of the game for which to retrieve the project file. If not specified,
    /// all versions are considered.
    /// </param>
    /// <param name="loader">
    /// Optional. The loader used for the mod. If not specified, all loaders are considered.
    /// </param>
    /// <returns>
    /// The latest project file associated with the specified project, game version, and loader (if
    /// provided), or null if no such project file is found.
    /// </returns>
    /// <remarks>
    /// This method fetches project files for the provided project ID and filters them based on the
    /// specified game version (if provided) and loader (if provided). It then returns the latest
    /// project file based on the date of the file.
    /// </remarks>
    public async Task<ModFile?> GetLatestProjectFile(string id, string? gameVersion = null, ModLoaders? loader = null)
    {
        ModFile[]? files = await GetProjectFiles(id);

        if (files != null && files.Any())
        {
            return files.OrderByDescending(i => i.FileDate).FirstOrDefault(i => (gameVersion == null || i.GameVersions.Contains(gameVersion)) && (loader == null || i.GameVersions.Contains(loader.ToString())));
        }

        return null;
    }

    /// <summary>
    /// Retrieves detailed information about a specific project (mod, modpack, resource pack, or
    /// world) asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the project.</param>
    /// <param name="classId">The class ID for the type of project.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains detailed information
    /// about the project.
    /// </returns>
    private async Task<CurseforgeProject?> GetProject(string id, int classId)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"{BASE_URI}mods/{id}?gameId={GAME_ID}&classId={classId}");
        request.Headers.Add("x-api-key", _api);
        return (await _client.GetAsJson(request))?["data"]?.ToObject<CurseforgeProject>();
    }

    /// <summary>
    /// Searches for content that matches the specified criteria asynchronously.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="classId">The class ID for the type of content to search for.</param>
    /// <param name="gameVersion">The version of the Minecraft game.</param>
    /// <param name="loader">The mod loader to filter by.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the search results for
    /// the specified content type.
    /// </returns>
    private async Task<CurseforgeSearchResult?> SearchAsync(string query, int classId, string gameVersion, ModLoaders? loader)
    {
        string url = $"{BASE_URI}mods/search?searchFilter={query}&gameId={GAME_ID}&classId={classId}&gameVersion={gameVersion}";
        if (loader != null)
        {
            url += $"&modLoaderType={loader}";
        }
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Add("x-api-key", _api);
        CurseforgeSearchResult? result = (await _client.GetAsJson(request))?.ToObject<CurseforgeSearchResult>();
        if (result != null)
        {
            return result.Value;
        }
        return null;
    }
}