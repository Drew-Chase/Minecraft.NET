/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Model;

public struct DownloadArtifact
{
    [JsonProperty("downloads")]
    public ArtifactDownloads Downloads { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("rules")]
    public Rule[] Rules { get; set; }
}

public struct ArtifactDownloads
{
    [JsonProperty("artifact")]
    public Artifact Artifact { get; set; }
}

public struct Artifact
{
    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("sha1")]
    public string Sha1 { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}

public struct Rule
{
    [JsonProperty("action")]
    public string Action { get; set; }

    [JsonProperty("os")]
    public OperatingSystem OS { get; set; }
}

public struct OperatingSystem
{
    [JsonProperty("name")]
    public string Name { get; set; }
}