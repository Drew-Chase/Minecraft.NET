/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Forge.Model;

public struct ModToml
{
    public string modLoader { get; set; }
    public string loaderVersion { get; set; }
    public string license { get; set; }
    public string issueTrackerURL { get; set; }
    public string displayURL { get; set; }
    public string logoFile { get; set; }
    public string authors { get; set; }
    public List<ModEntry> mods { get; set; }
}

public sealed class ModEntry
{
    public string modId { get; set; }
    public string version { get; set; }
    public string displayName { get; set; }
    public string updateJSONURL { get; set; }
    public string displayTest { get; set; }
    public string description { get; set; }
    public List<Dependency> dependencies { get; set; }
}

public sealed class Dependency
{
    public string modId { get; set; }
    public bool mandatory { get; set; }
    public string versionRange { get; set; }
    public string ordering { get; set; }
    public string side { get; set; }
}