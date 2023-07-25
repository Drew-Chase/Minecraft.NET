/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

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