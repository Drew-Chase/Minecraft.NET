// LFInteractive LLC. 2021-2024﻿
namespace Chase.Minecraft.Modrinth.Model;

public struct ModrinthProjectDependencies
{
    public ModrinthProject[] Projects { get; set; }
    public ModrinthVersionFile[] Versions { get; set; }
}