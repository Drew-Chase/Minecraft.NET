// LFInteractive LLC. 2021-2024﻿
using Chase.Networking;

namespace Chase.Minecraft.Curseforge;

public class CurseforgeClient : IDisposable
{
    private static readonly string BASE_URI = "api.curseforge.com";
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
}