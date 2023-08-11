/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Data;
using Chase.Minecraft.Model;

namespace Chase.Minecraft.Modpacks;

public class SearchBuilder
{
    internal readonly string query;
    internal readonly List<MinecraftVersion> minecraftVersions;
    internal readonly List<ModLoaders> loaders;
    internal readonly List<PlatformSource> platforms;
    internal int limit, offset;
    internal string author;
    internal Dictionary<PlatformSource, string> apiPlatform;

    public SearchBuilder(string query)
    {
        this.query = query;
        minecraftVersions = new();
        loaders = new();
        platforms = new();
        apiPlatform = new();
        limit = 10;
        offset = 0;
        author = "";
    }

    public SearchBuilder() : this("")
    {
    }

    public SearchBuilder AddMinecraftVersion(MinecraftVersion version)
    {
        minecraftVersions.Add(version);
        return this;
    }

    public SearchBuilder AddLoader(ModLoaders modLoaders)
    {
        loaders.Add(modLoaders);
        return this;
    }

    public SearchBuilder AddPlatform(PlatformSource platform, string api_key = "")
    {
        platforms.Add(platform);
        if (!string.IsNullOrEmpty(api_key))
        {
            apiPlatform[platform] = api_key;
        }
        return this;
    }

    public SearchBuilder SetLimit(int limit)
    {
        this.limit = limit;
        return this;
    }

    public SearchBuilder SetOffset(int offset)
    {
        this.offset = offset;
        return this;
    }

    public SearchBuilder SetAuthor(string author)
    {
        this.author = author;
        return this;
    }
}