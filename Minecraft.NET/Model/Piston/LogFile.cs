/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Model.Piston;

using Newtonsoft.Json;

public struct LogFile
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("sha1")]
    public string Sha1 { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}