/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Curseforge.Model;
using Chase.Minecraft.Model;
using Chase.Networking;
using Chase.Networking.Event;

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

    public Task<CurseforgeProject?> GetMod(string id) => GetProject(id, MODS_SECTION_ID);

    public Task<CurseforgeProject?> GetModpack(string id) => GetProject(id, MODPACKS_SECTION_ID);

    public Task<CurseforgeProject?> GetResourcepack(string id) => GetProject(id, RESOURCE_PACKS_SECTION_ID);

    public Task<CurseforgeProject?> GetWorld(string id) => GetProject(id, WORLDS_SECTION_ID);

    public Task<string> Download(ModFile mod, InstanceModel instance, string subfolder = "mods", DownloadProgressEvent? progressEvent = null) => Download(mod, Path.Combine(instance.Path, subfolder), progressEvent);

    public async Task<string> Download(ModFile mod, string directory, DownloadProgressEvent? progressEvent = null)
    {
        string path = Path.Combine(directory, mod.FileName);
        await _client.DownloadFileAsync(mod.DownloadUrl, path, progressEvent);
        return path;
    }

    public Task<ModFile[]?> GetModFiles(string id) => GetProjectFiles(id, MODS_SECTION_ID);

    public Task<ModFile[]?> GetModpackFiles(string id) => GetProjectFiles(id, MODPACKS_SECTION_ID);

    public Task<ModFile[]?> GetResourcepackFiles(string id) => GetProjectFiles(id, RESOURCE_PACKS_SECTION_ID);

    public Task<ModFile[]?> GetWorldFiles(string id) => GetProjectFiles(id, WORLDS_SECTION_ID);

    public Task<ModFile?> GetModFile(string id, string fileId) => GetProjectFile(id, fileId, MODS_SECTION_ID);

    public Task<ModFile?> GetModpackFile(string id, string fileId) => GetProjectFile(id, fileId, MODPACKS_SECTION_ID);

    public Task<ModFile?> GetResourcepackFile(string id, string fileId) => GetProjectFile(id, fileId, RESOURCE_PACKS_SECTION_ID);

    public Task<ModFile?> GetWorldFile(string id, string fileId) => GetProjectFile(id, fileId, WORLDS_SECTION_ID);

    private async Task<CurseforgeProject?> GetProject(string id, int classId) => (await _client.GetAsJson($"{BASE_URI}mods/{id}?gameId={GAME_ID}&classId={classId}"))?["data"]?.ToObject<CurseforgeProject>();

    private async Task<ModFile[]?> GetProjectFiles(string id, int classId) => (await _client.GetAsJson($"{BASE_URI}mods/{id}/files?gameId={GAME_ID}&classId={classId}"))?["data"]?.ToObject<ModFile[]>();

    private async Task<ModFile?> GetProjectFile(string id, string fileId, int classId) => (await _client.GetAsJson($"{BASE_URI}mods/{id}/files/{fileId}?gameId={GAME_ID}&classId={classId}"))?["data"]?.ToObject<ModFile>();

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