// LFInteractive LLC. 2021-2024﻿
namespace Chase.Minecraft.Model;

public struct ClientStartInfo
{
    public required string Directory { get; set; }

    public required string JVMArguments { get; set; }

    public string? OfflineUsername { get; set; } = null;

    public required RAMInfo RAM { get; set; }

    public ClientStartInfo()
    {
    }
}

public struct RAMInfo
{
    public int MaximumRamMB { get; set; }
    public int MinimumRamMB { get; set; }
    public int PermGenSizeMB { get; set; }
}