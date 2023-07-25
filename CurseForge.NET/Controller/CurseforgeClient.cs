﻿// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Curseforge.Model;
using Chase.Networking;
using Newtonsoft.Json.Linq;

namespace Chase.Minecraft.Curseforge.Controller;

public class CurseforgeClient : IDisposable
{
    private static readonly string BASE_URI = "https://api.curseforge.com/v1/";
    private static readonly int GAME_ID = 432;
    private static readonly int FORGE_MODLOADER_ID = 1;
    private static readonly int FABRIC_MODLOADER_ID = 4;
    private static readonly int FABRIC_MOD_ID = 306612;
    private static readonly int LEGACY_FABRIC_MOD_ID = 400281;
    private static readonly int JUMPLOADER_MOD_ID = 361988;
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

    public Task<CurseforgeSearchResult?> SearchWorldsAsync(string query, string gameVersion, ModLoaders loaders) => SearchAsync(query, WORLDS_SECTION_ID, gameVersion, loaders);

    public Task<CurseforgeSearchResult?> SearchResourcepacksAsync(string query, string gameVersion, ModLoaders loaders) => SearchAsync(query, RESOURCE_PACKS_SECTION_ID, gameVersion, loaders);

    private async Task<CurseforgeSearchResult?> SearchAsync(string query, int classId, string gameVersion, ModLoaders loader)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"{BASE_URI}mods/search?searchFilter={query}&gameId={GAME_ID}&classId={classId}&modLoaderType={loader}&gameVersion={gameVersion}");
        request.Headers.Add("x-api-key", _api);
        HttpResponseMessage response = await _client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            CurseforgeSearchResult? result = JObject.Parse(await response.Content.ReadAsStringAsync())?.ToObject<CurseforgeSearchResult>();
            if (result != null)
            {
                return result.Value;
            }
        }
        return null;
    }
}