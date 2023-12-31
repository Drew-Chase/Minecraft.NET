﻿/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Curseforge.Model;

public struct CurseforgeSearchResult
{
    [JsonProperty("data")]
    public CurseforgeProject[] Projects { get; set; }

    [JsonProperty("pagination")]
    public CurseforgeSearchPagination Pagination { get; set; }
}