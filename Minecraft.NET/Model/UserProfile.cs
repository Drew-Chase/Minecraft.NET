/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Model;

public struct UserProfile
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("skins")]
    public Skin[] Skins { get; set; }

    [JsonProperty("capes")]
    public Cape[] Capes { get; set; }
}

public class Skin
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("textureKey")]
    public string TextureKey { get; set; }

    [JsonProperty("variant")]
    public string Variant { get; set; }
}

public class Cape
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("alias")]
    public string Alias { get; set; }
}