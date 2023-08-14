/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Data;

namespace Chase.Minecraft.Modrinth.Model;

public struct ModrinthSearchQuery
{
    public string Query { get; set; } = "";

    public SearchOrdering Ordering { get; set; } = SearchOrdering.Relevance;

    public int Limit { get; set; } = 10;

    public int Offset { get; set; } = 0;

    public FacetBuilder? Facets { get; set; } = null;

    public ModrinthSearchQuery()
    {
    }
}