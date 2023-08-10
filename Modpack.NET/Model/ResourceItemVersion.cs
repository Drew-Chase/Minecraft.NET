/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Modpacks.Data;

namespace Chase.Minecraft.Modpacks.Model;

public abstract class ResourceItemVersion
{
    public string ID { get; set; }
    public string Changelog { get; set; }
    public int Downloads { get; set; }
    public string Version { get; set; }
    public string[] GameVersions { get; set; }
    public ReleaseType ReleaseType { get; set; }
    public DateTime ReleaseDate { get; set; }
}