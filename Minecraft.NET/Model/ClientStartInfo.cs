/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Model;

public struct ClientStartInfo
{
    public string Name { get; set; } = "";
    public required string Directory { get; set; }

    public string JVMArguments { get; set; } = "";

    public required string JavaExecutable { get; set; }

    public required string Username { get; set; }

    public RAMInfo RAM { get; set; }

    public ClientStartInfo()
    {
    }
}

public struct RAMInfo
{
    public int MaximumRamMB { get; set; } = 4096;

    public int MinimumRamMB { get; set; } = 4096;

    public int PermGenSizeMB { get; set; } = 128;

    public RAMInfo()
    {
    }
}