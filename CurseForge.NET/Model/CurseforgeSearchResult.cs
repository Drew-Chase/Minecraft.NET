// LFInteractive LLC. 2021-2024﻿
using Newtonsoft.Json;

namespace Chase.Minecraft.Curseforge.Model;

public struct CurseforgeSearchResult
{
    [JsonProperty("data")]
    public CurseforgeProject[] Projects { get; set; }

    [JsonProperty("pagination")]
    public CurseforgeSearchPagination Pagination { get; set; }
}

public struct CurseforgeSearchPagination
{
    public int Index { get; set; }
    public int PageSize { get; set; }
    public int ResultCount { get; set; }
    public int TotalCount { get; set; }
}