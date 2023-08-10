/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Data;

namespace Chase.Minecraft.Modpacks.Model;

public abstract class ResourceItemBase
{
    public string ID { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public int Downloads { get; set; }
    public string Icon { get; set; }
    public string Banner { get; set; }
    public ResourceItemVersion[] Versions { get; set; }
    public string[] GameVersions { get; set; }
    public string[] Categories { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public PlatformSource Platform { get; set; }
    public bool IsDistributionAllowed { get; set; }
}