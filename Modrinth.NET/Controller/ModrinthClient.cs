/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Model;
using Chase.Minecraft.Modrinth.Model;
using Chase.Networking;
using Chase.Networking.Event;
using Newtonsoft.Json.Linq;

namespace Chase.Minecraft.Modrinth.Controller;

public sealed class ModrinthClient : IDisposable
{
    private readonly string _api = "";
    private readonly string BASE_URL = "https://api.modrinth.com/v2/";
    private readonly NetworkClient _client;

    public ModrinthClient()
    {
        _client = new NetworkClient();
    }

    private ModrinthClient(string api_key) : this()
    {
        _api = api_key;
    }

    /// <summary>
    /// Searches for mods or projects in Modrinth based on the provided search query.
    /// </summary>
    /// <param name="query">The search query containing search parameters.</param>
    /// <returns>
    /// A task representing the asynchronous search operation. The result is the search result as a
    /// ModrinthSearchResult object, or null if the search was not successful.
    /// </returns>
    public async Task<ModrinthSearchResult?> SearchAsync(ModrinthSearchQuery query)
    {
        string facets = "";
        if (query.Facets != null && !query.Facets.IsEmpty)
        {
            facets = $"&{query.Facets.Build()}";
        }
        Uri uri = new($"{BASE_URL}search?query={query.Query}&limit={query.Limit}&index={query.Ordering.ToString().ToLower()}&offset={query.Offset}{facets}");
        HttpResponseMessage response = await _client.GetAsync(uri);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                string content = await response.Content.ReadAsStringAsync();
                return JObject.Parse(content).ToObject<ModrinthSearchResult>();
            }
            catch
            {
            }
        }

        return null;
    }

    /// <summary>
    /// Synchronously searches for mods or projects in Modrinth based on the provided search query.
    /// </summary>
    /// <param name="query">The search query containing search parameters.</param>
    /// <returns>
    /// The search result as a ModrinthSearchResult object, or null if the search was not successful.
    /// </returns>
    public ModrinthSearchResult? Search(ModrinthSearchQuery query) => SearchAsync(query).Result;

    /// <summary>
    /// Retrieves information about a specific project from Modrinth.
    /// </summary>
    /// <param name="id">The ID of the project to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result is the project information as a
    /// ModrinthProject object, or null if the retrieval was not successful.
    /// </returns>
    public async Task<ModrinthProject?> GetProjectAsync(string id)
    {
        HttpResponseMessage response = await _client.GetAsync($"{BASE_URL}project/{id}");
        if (response.IsSuccessStatusCode)
        {
            return JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<ModrinthProject>();
        }
        return null;
    }

    /// <summary>
    /// Synchronously retrieves information about a specific project from Modrinth.
    /// </summary>
    /// <param name="id">The ID of the project to retrieve.</param>
    /// <returns>
    /// The project information as a ModrinthProject object, or null if the retrieval was not successful.
    /// </returns>
    public ModrinthProject? GetProject(string id) => GetProjectAsync(id).Result;

    /// <summary>
    /// Retrieves dependencies of a specific project from Modrinth.
    /// </summary>
    /// <param name="id">The ID of the project for which to retrieve dependencies.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result is the project dependencies as a
    /// ModrinthProjectDependencies object, or null if the retrieval was not successful.
    /// </returns>
    public async Task<ModrinthProjectDependencies?> GetProjectDependenciesAsync(string id)
    {
        HttpResponseMessage response = await _client.GetAsync($"{BASE_URL}project/{id}/dependencies");
        if (response.IsSuccessStatusCode)
        {
            return JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<ModrinthProjectDependencies>();
        }
        return null;
    }

    /// <summary>
    /// Synchronously retrieves dependencies of a specific project from Modrinth.
    /// </summary>
    /// <param name="id">The ID of the project for which to retrieve dependencies.</param>
    /// <returns>
    /// The project dependencies as a ModrinthProjectDependencies object, or null if the retrieval
    /// was not successful.
    /// </returns>
    public ModrinthProjectDependencies? GetProjectDependencies(string id) => GetProjectDependenciesAsync(id).Result;

    /// <summary>
    /// Retrieves information about a specific user from Modrinth.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result is the user information as a
    /// ModrinthUser object, or null if the retrieval was not successful.
    /// </returns>
    public async Task<ModrinthUser?> GetUserAsync(string id)
    {
        HttpResponseMessage response = await _client.GetAsync($"{BASE_URL}user/{id}");
        if (response.IsSuccessStatusCode)
        {
            ModrinthUser? user = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<ModrinthUser>();
            if (user != null)
            {
                user.Projects = (await _client.GetAsJsonArray($"{BASE_URL}user/{id}/projects"))?.ToObject<ModrinthProject[]>() ?? Array.Empty<ModrinthProject>();
                return user;
            }
        }
        return null;
    }

    /// <summary>
    /// Synchronously retrieves information about a specific user from Modrinth.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>
    /// The user information as a ModrinthUser object, or null if the retrieval was not successful.
    /// </returns>
    public ModrinthUser? GetUser(string id) => GetUserAsync(id).Result;

    /// <summary>
    /// Gets the versions of a Modrinth project based on the provided project ID.
    /// </summary>
    /// <param name="id">The ID of the project for which to retrieve the versions.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation with the project versions.
    /// </returns>
    public async Task<ModrinthVersionFile[]?> GetProjectVersionsAsync(string id, string[]? gameVersions = null, ModLoaders[]? loaders = null)
    {
        string gameVersion = "";
        if (gameVersions != null && gameVersions.Any())
        {
            gameVersion = $"?game_versions=[";
            foreach (string version in gameVersions)
            {
                gameVersion += $"\"{version}\",";
            }
            gameVersion = gameVersion.Trim(',').Trim() + "]";
        }
        string loader = "";
        if (loaders != null && loaders.Any())
        {
            if (gameVersions.Any())
            {
                loader = "&";
            }
            else
            {
                loader = "?";
            }
            loader += "loaders=[";
            foreach (ModLoaders l in loaders)
            {
                loader += $"\"{l}\",";
            }
            loader = loader.Trim(',').Trim() + "]";
        }
        HttpResponseMessage response = await _client.GetAsync($"{BASE_URL}project/{id}/version{gameVersion}{loader}");
        if (response.IsSuccessStatusCode)
        {
            return JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<ModrinthVersionFile[]>();
        }
        return null;
    }

    /// <summary>
    /// Retrieves the latest version file for a specific project, optionally filtered by Minecraft
    /// version and loader.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="minecraftVersion">
    /// Optional. The version of Minecraft for which to retrieve the version file. If not specified,
    /// all versions are considered.
    /// </param>
    /// <param name="loader">
    /// Optional. The loader used for the mod. If not specified, all loaders are considered.
    /// </param>
    /// <returns>
    /// The latest version file associated with the specified project, Minecraft version, and loader
    /// (if provided), or null if no such version file is found.
    /// </returns>
    /// <remarks>
    /// This method fetches project versions that match the provided Minecraft version (if provided)
    /// and loader (if provided), and then returns the latest version file based on the date of publication.
    /// </remarks>

    public async Task<ModrinthVersionFile?> GetLatestVersionFile(string projectId, string? minecraftVersion = null, ModLoaders? loader = null)
    {
        ModrinthVersionFile[]? versions = await GetProjectVersionsAsync(projectId, minecraftVersion == null ? null : new string[] { minecraftVersion }, loader == null ? null : new ModLoaders[] { loader.Value });
        if (versions != null && versions.Any())
        {
            return versions.OrderByDescending(i => i.DatePublished).FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// Gets the versions of a Modrinth project based on the provided project ID.
    /// </summary>
    /// <param name="id">The ID of the project for which to retrieve the versions.</param>
    /// <param name="gameVersions">an array of acceptable minecraft versions</param>
    /// <param name="loaders">an array of acceptable mod loaders.</param>
    /// <returns>The project versions, or null if the operation fails.</returns>
    public ModrinthVersionFile[]? GetProjectVersions(string id, string[]? gameVersions = null, ModLoaders[]? loaders = null) => GetProjectVersionsAsync(id, gameVersions, loaders).Result;

    /// <summary>
    /// Downloads a version file and adds it to the specified instance.
    /// </summary>
    /// <param name="versionFile">The details of the version file to be downloaded.</param>
    /// <param name="instance">The instance to which the downloaded file will be added.</param>
    /// <param name="subpath">
    /// The subpath within the instance directory where the file will be stored. Default is "mods".
    /// </param>
    /// <param name="downloadProgress">
    /// An event handler to track the progress of the download. Default is an empty event handler.
    /// </param>
    /// <returns>The full path to the downloaded version file.</returns>
    /// <remarks>
    /// This method downloads a version file specified by the provided <paramref
    /// name="versionFile"/> details and adds it to the given <paramref name="instance"/>. The
    /// <paramref name="subpath"/> parameter can be used to specify a subdirectory within the
    /// instance's path to store the downloaded file. An optional <paramref
    /// name="downloadProgress"/> event handler can be provided to track the progress of the download.
    /// </remarks>
    public async Task<string> DownloadVersionFile(VersionFileDetails versionFile, InstanceModel instance, string subpath = "mods", DownloadProgressEvent? downloadProgress = null)
    {
        downloadProgress ??= (s, e) => { };
        string path = Path.Combine(Directory.CreateDirectory(Path.Combine(instance.Path, subpath)).FullName, versionFile.Filename);
        instance.InstanceManager.AddMod(instance, new()
        {
            Source = Minecraft.Data.PlatformSource.Modrinth,
            DownloadURL = versionFile.Url,
            Name = versionFile.Filename,
        });
        await _client.DownloadFileAsync(versionFile.Url, path, downloadProgress);

        return path;
    }

    /// <summary>
    /// Asynchronously gets the categories available in Modrinth.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The result is an array of <see
    /// cref="CategoryTag"/> representing the categories available in Modrinth, or null if the
    /// retrieval was not successful.
    /// </returns>
    public async Task<CategoryTag[]?> GetCategoriesAsync()
    {
        HttpResponseMessage response = await _client.GetAsync($"{BASE_URL}tag/category");
        if (response.IsSuccessStatusCode)
        {
            return JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<CategoryTag[]>();
        }
        return null;
    }

    /// <summary>
    /// Retrieves information about a specific project version from Modrinth API asynchronously.
    /// </summary>
    /// <param name="project_id">The unique identifier of the project.</param>
    /// <param name="version_id">The unique identifier of the project version.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result
    /// contains the information about the project version as a <see cref="ModrinthVersionFile"/>
    /// object, or null if the version is not found.
    /// </returns>
    public async Task<ModrinthVersionFile?> GetProjectVersionAsync(string project_id, string version_id) => (await _client.GetAsJson($"{BASE_URL}project/{project_id}/version/{version_id}"))?.ToObject<ModrinthVersionFile>();

    /// <summary>
    /// Retrieves all project versions that match a specific Minecraft version from Modrinth API asynchronously.
    /// </summary>
    /// <param name="project_id">The unique identifier of the project.</param>
    /// <param name="minecraftVersion">The Minecraft version to filter the project versions by.</param>
    /// <param name="ignorePatchVersion">
    /// Flag indicating whether to ignore the patch version of the Minecraft version. If true, only
    /// the major and minor versions are considered for matching.
    /// </param>
    /// <param name="requiredLoader">
    /// If set, this will only return version files with a specific mod loader
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result
    /// contains an array of <see cref="ModrinthVersionFile"/> objects that match the specified
    /// Minecraft version.
    /// </returns>
    public async Task<ModrinthVersionFile[]> GetModrinthVersionsByMinecraftVersionAsync(string project_id, MinecraftVersion minecraftVersion, bool ignorePatchVersion = false, ModLoaders? requiredLoader = null)
    {
        List<ModrinthVersionFile> versions = new();
        ModrinthVersionFile[]? projectVerions = await GetProjectVersionsAsync(project_id);
        if (projectVerions != null && projectVerions.Any())
        {
            foreach (ModrinthVersionFile versionFile in projectVerions)
            {
                if (requiredLoader != null && !versionFile.Loaders.Contains(requiredLoader.Value.ToString(), StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (string gameVersion in versionFile.GameVersions)
                {
                    if (Version.TryParse(gameVersion, out Version? parsedModGameVersion) && parsedModGameVersion != null && Version.TryParse(minecraftVersion.ID, out Version? parsedGameVersion) && parsedGameVersion != null)
                    {
                        if (parsedGameVersion.Major == parsedModGameVersion.Major && parsedGameVersion.Minor == parsedModGameVersion.Minor)
                        {
                            if (ignorePatchVersion)
                            {
                                versions.Add(versionFile);
                            }
                            else
                            {
                                if (parsedGameVersion.Build == parsedModGameVersion.Build)
                                {
                                    versions.Add(versionFile);
                                }
                            }
                        }
                    }
                }
            }
        }
        return versions.OrderByDescending(i => i.DatePublished).ToArray();
    }

    /// <summary>
    /// Gets the categories available in Modrinth.
    /// </summary>
    /// <returns>
    /// An array of <see cref="CategoryTag"/> representing the categories available in Modrinth, or
    /// null if the retrieval was not successful.
    /// </returns>
    public CategoryTag[]? GetCategories() => GetCategoriesAsync().Result;

    public void Dispose()
    {
        _client.Dispose();
    }
}