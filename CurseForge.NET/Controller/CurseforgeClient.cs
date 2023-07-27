/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Curseforge.Model;
using Chase.Networking;

namespace Chase.Minecraft.Curseforge.Controller;

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

    public CurseforgeClient(string api_key)
    {
        _client = new();
        _api = api_key;
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public Task<CurseforgeSearchResult?> SearchModsAsync(string query, string gameVersion, ModLoaders loaders) => SearchAsync(query, MODS_SECTION_ID, gameVersion, loaders);

    public Task<CurseforgeSearchResult?> SearchModpackAsync(string query, string gameVersion, ModLoaders loaders) => SearchAsync(query, MODPACKS_SECTION_ID, gameVersion, loaders);

    public Task<CurseforgeSearchResult?> SearchWorldsAsync(string query, string gameVersion) => SearchAsync(query, WORLDS_SECTION_ID, gameVersion, null);

    public Task<CurseforgeSearchResult?> SearchResourcepacksAsync(string query, string gameVersion) => SearchAsync(query, RESOURCE_PACKS_SECTION_ID, gameVersion, null);

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