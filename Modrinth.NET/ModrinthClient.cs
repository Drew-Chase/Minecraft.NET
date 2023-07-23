// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Modrinth.Model;
using Chase.Networking;
using Newtonsoft.Json.Linq;

namespace Chase.Minecraft.Modrinth;

public sealed class ModrinthClient : NetworkClient
{
    private readonly string API_KEY = "";
    private readonly string BaseURL = "https://api.modrinth.com/v2/";

    public ModrinthClient()
    {
    }

    private ModrinthClient(string api_key) : this()
    {
        API_KEY = api_key;
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
        Uri uri = new($"{BaseURL}search?query={query.Query}&limit={query.Limit}&index={query.Ordering.ToString().ToLower()}&offset={query.Offset}{facets}");
        HttpResponseMessage response = await GetAsync(uri);

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
        HttpResponseMessage response = await GetAsync($"{BaseURL}project/{id}");
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
        HttpResponseMessage response = await GetAsync($"{BaseURL}project/{id}/dependencies");
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
        HttpResponseMessage response = await GetAsync($"{BaseURL}user/{id}");
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
}