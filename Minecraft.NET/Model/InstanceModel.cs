/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Model;

public sealed class InstanceModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    public string Path { get; set; }
    public string? Image { get; set; } = null;
    public string Java { get; set; }
    public string JVMArguments { get; set; } = "";
    public string MinecraftArguments { get; set; } = "";
    public RAMInfo RAM { get; set; } = new();
    public ModLoaderModel ModLoader { get; set; }
    public MinecraftVersion MinecraftVersion { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;
    public string ClientJar { get; set; } = "";
}