// LFInteractive LLC. 2021-2024﻿
namespace Chase.Minecraft.Model;

public struct MinecraftVersionManifest
{
    public LatestReleases Latest { get; set; }
    public MinecraftVersion[] Versions { get; set; }
}

public struct LatestReleases
{
    public string Release { get; set; }
    public string Snapshot { get; set; }
}

public struct MinecraftVersion
{
    public string ID { get; set; }
    public string Type { get; set; }
    public Uri URL { get; set; }
    public DateTime Time { get; set; }
    public DateTime ReleaseTime { get; set; }
}