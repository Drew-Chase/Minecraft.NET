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
            return JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<ModrinthUser>();
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
    public async Task<ModrinthVersionFile[]?> GetProjectVersionsAsync(string id)
    {
        HttpResponseMessage response = await _client.GetAsync($"{BASE_URL}project/{id}/version");
        if (response.IsSuccessStatusCode)
        {
            return JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<ModrinthVersionFile[]>();
        }
        return null;
    }

    /// <summary>
    /// Gets the versions of a Modrinth project based on the provided project ID.
    /// </summary>
    /// <param name="id">The ID of the project for which to retrieve the versions.</param>
    /// <returns>The project versions, or null if the operation fails.</returns>
    public ModrinthVersionFile[]? GetProjectVersions(string id) => GetProjectVersionsAsync(id).Result;

    /// <summary>
    /// Downloads a specific version file from Modrinth and saves it to the specified output directory.
    /// </summary>
    /// <param name="versionFile">The version file details.</param>
    /// <param name="outputDirectory">The directory where the file will be saved.</param>
    /// <param name="downloadProgress">The event to track the download progress.</param>
    /// <param name="subpath">
    /// The folder inside of the instance directory that the file will be downloaded to.
    /// </param>
    /// <returns>A Task containing the path to the downloaded file.</returns>
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