/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Model.Piston;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public struct PistonModel
{
    [JsonProperty("arguments")]
    public Arguments Arguments { get; set; }

    [JsonProperty("assetIndex")]
    public AssetIndex AssetIndex { get; set; }

    [JsonProperty("assets")]
    public string Assets { get; set; }

    [JsonProperty("complianceLevel")]
    public int ComplianceLevel { get; set; }

    [JsonProperty("downloads")]
    public Downloads Downloads { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("javaVersion")]
    public JavaVersion JavaVersion { get; set; }

    [JsonProperty("libraries")]
    public List<Library> Libraries { get; set; }

    [JsonProperty("logging")]
    public Logging Logging { get; set; }

    [JsonProperty("mainClass")]
    public string MainClass { get; set; }

    [JsonProperty("minimumLauncherVersion")]
    public int MinimumLauncherVersion { get; set; }

    [JsonProperty("releaseTime")]
    public DateTime ReleaseTime { get; set; }

    [JsonProperty("time")]
    public DateTime Time { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}