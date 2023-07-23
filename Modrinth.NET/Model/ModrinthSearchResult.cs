// LFInteractive LLC. 2021-2024﻿
using Newtonsoft.Json;

namespace Chase.Minecraft.Modrinth.Model;

public struct ModrinthSearchResult
{
    public ModrinthSearchResultItem[] Hits { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }

    [JsonProperty("total_hits")]
    public int TotalHits { get; set; }
}