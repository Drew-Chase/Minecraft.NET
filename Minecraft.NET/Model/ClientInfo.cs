/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Model;

internal sealed class ClientInfo
{
    public ClientStartInfo ClientStartInfo { get; set; }
    public string Assets { get; set; }
    public string AssetIndex { get; set; }
    public string Libraries { get; set; }
    public string ClientJar { get; set; }
    public string AuthenticationToken { get; set; } = "0";
    public string InstanceDirectory { get; set; }
    public string ClientID { get; set; } = "0";
    public string ClientVersion { get; set; } = "0.0.0";
    public string ClientName { get; set; } = "Minecraft.NET";
    public MinecraftVersion Version { get; set; }
    public DownloadArtifact[] LibraryFiles { get; set; }
}