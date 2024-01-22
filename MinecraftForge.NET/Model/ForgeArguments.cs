/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Forge.Model;

public sealed class ForgeArguments
{
    public ForgeArguments()
    {
    }

    [JsonProperty("game")]
    public string[] Game { get; set; } = Array.Empty<string>();

    [JsonProperty("jvm")]
    public string[] Jvm { get; set; } = Array.Empty<string>();
}
