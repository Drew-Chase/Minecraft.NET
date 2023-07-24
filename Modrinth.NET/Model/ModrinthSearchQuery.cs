// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Modrinth.Data;

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