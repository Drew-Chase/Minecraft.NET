// LFInteractive LLC. 2021-2024﻿
using Newtonsoft.Json;

namespace Chase.Minecraft.Modrinth.Model;

public struct CategoryTag
{
    [JsonProperty("icon")]
    public string Icon { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("project_type")]
    public string ProjectType { get; set; }

    [JsonProperty("header")]
    public string Header { get; set; }
}