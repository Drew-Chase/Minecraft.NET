/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Model;

/// <summary>
/// A data set for Minimum and Maximum Ram
/// </summary>
public sealed class RAMInfo
{
    /// <summary>
    /// Maximum ram allocation in megabytes
    /// </summary>
    [JsonProperty("max-ram")]
    public int Maximum { get; set; } = 4096;

    /// <summary>
    /// Minimum ram allocation in megabytes
    /// </summary>
    [JsonProperty("min-ram")]
    public int Minimum { get; set; } = 4096;
}