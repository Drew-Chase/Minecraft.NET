/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Curseforge.Model;

public struct Links
{
    [JsonProperty("websiteUrl")]
    public string WebsiteUrl { get; set; }

    [JsonProperty("wikiUrl")]
    public string WikiUrl { get; set; }

    [JsonProperty("issuesUrl")]
    public string IssuesUrl { get; set; }

    [JsonProperty("sourceUrl")]
    public string SourceUrl { get; set; }
}
