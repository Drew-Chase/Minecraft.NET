// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Modrinth.Model;
using Chase.Networking;
using Newtonsoft.Json.Linq;

namespace Chase.Minecraft.Modrinth;

public sealed class ModrinthClient : NetworkClient
{
    private readonly string API_KEY = "";
    private readonly string BaseURL = "https://api.modrinth.com/v2/";

    public ModrinthClient(string api_key) : this()
    {
        API_KEY = api_key;
    }

    public ModrinthClient()
    {
    }

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

    public ModrinthSearchResult? Search(ModrinthSearchQuery query) => SearchAsync(query).Result;
}