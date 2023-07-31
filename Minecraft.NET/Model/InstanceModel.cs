/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Instances;
using Newtonsoft.Json;

namespace Chase.Minecraft.Model;

public sealed class InstanceModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    public string Path { get; set; }
    public string? Image { get; set; } = null;
    public string GameVersion { get; set; }
    public string Java { get; set; }
    public string[] JVMArguments { get; set; } = Array.Empty<string>();
    public string[] MinecraftArguments { get; set; } = Array.Empty<string>();
    public string[] AdditionalClassPaths { get; set; } = Array.Empty<string>();
    public string LaunchClassPath { get; set; } = "net.minecraft.client.main.Main";
    public RAMInfo RAM { get; set; } = new();
    public ModLoaderModel ModLoader { get; set; }
    public ModModel[] Mods { get; set; } = Array.Empty<ModModel>();
    public MinecraftVersion MinecraftVersion { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;
    public string ClientJar { get; set; } = "";
    public int WindowWidth { get; set; } = 854;
    public int WindowHeight { get; set; } = 480;

    [JsonIgnore]
    public InstanceManager InstanceManager { get; set; }

    public string[] ClassPaths { get; set; }
}