/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Forge.Model;

public enum ModOrdering
{
    NONE,
    BEFORE,
    AFTER,
}

public enum Side
{
    BOTH,
    CLIENT,
    SERVER,
}

public struct ModToml
{
    public string ModId { get; set; }
    public string Version { get; set; }
    public string License { get; set; }
    public string DisplayName { get; set; }
    public Uri? UpdateJsonUrl { get; set; }
    public string DisplayUrl { get; set; }
    public string IssueTrackerUrl { get; set; }
    public string LogoFile { get; set; }
    public string Authors { get; set; }
    public string Description { get; set; }
    public Dependency[] Dependencies { get; set; }
}

public sealed class Dependency
{
    public string ModId { get; set; }
    public bool Mandatory { get; set; }
    public string VersionRange { get; set; }
    public ModOrdering Ordering { get; set; }
    public Side Side { get; set; }
}