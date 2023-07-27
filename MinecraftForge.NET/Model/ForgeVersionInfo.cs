/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Forge.Model;

public struct ForgeVersionInfo
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("time")]
    public DateTime Time { get; set; }

    [JsonProperty("releaseTime")]
    public DateTime ReleaseTime { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("mainClass")]
    public string MainClass { get; set; }

    [JsonProperty("inheritsFrom")]
    public string InheritsFrom { get; set; }

    [JsonProperty("arguments")]
    public ForgeArguments Arguments { get; set; }

    [JsonProperty("libraries")]
    public ForgeLibrary[] Libraries { get; set; }
}