﻿/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Model.Piston;

using Newtonsoft.Json;

public struct Downloads
{
    [JsonProperty("client")]
    public Download Client { get; set; }

    [JsonProperty("client_mappings")]
    public Download ClientMappings { get; set; }

    [JsonProperty("server")]
    public Download Server { get; set; }

    [JsonProperty("server_mappings")]
    public Download ServerMappings { get; set; }
}
