// LFInteractive LLC. 2021-2024﻿
namespace Chase.Minecraft.Modrinth.Model;

public struct ModrinthSearchQuery
{
    public string Query { get; set; }
    public SearchOrdering Ordering { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public ModLoaders[] Modloaders { get; set; }
}