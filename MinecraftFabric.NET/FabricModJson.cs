/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Fabric;

public struct FabricModJson
{
    [JsonProperty("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("icon")]
    public string Icon { get; set; }

    [JsonProperty("authors")]
    public string[] Authors { get; set; }

    [JsonProperty("contact")]
    public ContactInfo Contact { get; set; }

    [JsonProperty("license")]
    public string License { get; set; }

    [JsonProperty("environment")]
    public string Environment { get; set; }

    [JsonProperty("entrypoints")]
    public EntryPoints EntryPoints { get; set; }

    [JsonProperty("depends")]
    public Dependencies Depends { get; set; }

    [JsonProperty("recommends")]
    public Recommends Recommends { get; set; }

    [JsonProperty("accessWidener")]
    public string AccessWidener { get; set; }

    [JsonProperty("mixins")]
    public string[] Mixins { get; set; }

    [JsonProperty("jars")]
    public JarInfo[] Jars { get; set; }
}

public struct ContactInfo
{
    [JsonProperty("homepage")]
    public string Homepage { get; set; }

    [JsonProperty("sources")]
    public string Sources { get; set; }

    [JsonProperty("issues")]
    public string Issues { get; set; }
}

public struct EntryPoints
{
    [JsonProperty("client")]
    public string[] Client { get; set; }

    [JsonProperty("modmenu")]
    public string[] ModMenu { get; set; }
}

public struct Dependencies
{
    [JsonProperty("fabricloader")]
    public string FabricLoader { get; set; }

    [JsonProperty("minecraft")]
    public string Minecraft { get; set; }
}

public struct Recommends
{
    [JsonProperty("modmenu")]
    public string ModMenu { get; set; }
}

public struct JarInfo
{
    [JsonProperty("file")]
    public string File { get; set; }
}