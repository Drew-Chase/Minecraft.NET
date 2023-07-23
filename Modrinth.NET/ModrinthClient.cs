// LFInteractive LLC. 2021-2024﻿
using Chase.Networking;

namespace Chase.Minecraft.Modrinth;

public abstract class ModrinthClient : NetworkClient
{
    private readonly string API_KEY = "";

    public ModrinthClient(string api_key) : this()
    {
        API_KEY = api_key;
    }

    public ModrinthClient()
    {
    }
}