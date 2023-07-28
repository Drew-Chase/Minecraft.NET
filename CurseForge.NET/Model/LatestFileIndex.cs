/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Curseforge.Model;

public struct LatestFileIndex
{
    [JsonProperty("gameVersion")]
    public string GameVersion { get; set; }

    [JsonProperty("fileId")]
    public int FileId { get; set; }

    [JsonProperty("filename")]
    public string Filename { get; set; }

    [JsonProperty("releaseType")]
    public int ReleaseType { get; set; }

    [JsonProperty("gameVersionTypeId")]
    public int GameVersionTypeId { get; set; }

    [JsonProperty("modLoader")]
    public int ModLoader { get; set; }
}