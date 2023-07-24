// LFInteractive LLC. 2021-2024﻿
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
    public MinecraftVersion Version { get; set; }
}