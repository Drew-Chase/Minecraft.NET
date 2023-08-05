/*
    PolygonMC - LFInteractive LLC. 2021-2024
    PolygonMC is a free and open source Minecraft Launcher implementing various modloaders, mod platforms, and minecraft authentication.
    PolygonMC is protected under GNU GENERAL PUBLIC LICENSE version 3.0 License
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
    https://github.com/DcmanProductions/PolygonMC
*/

using Chase.Minecraft.Data;
using Chase.Minecraft.Instances;
using Newtonsoft.Json;

namespace Chase.Minecraft.Model;

/// <summary>
/// Structure of an instance
/// </summary>
public sealed class InstanceModel
{
    /// <summary>
    /// The unique instance id
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The instance's name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The instance's description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The directory that the instance resides in
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// A instance image as base64
    /// </summary>
    public string? Image { get; set; } = null;

    /// <summary>
    /// The minecraft game version
    /// </summary>
    public string GameVersion { get; set; }

    /// <summary>
    /// The direct path to java executable
    /// </summary>
    public string Java { get; set; }

    /// <summary>
    /// The additional Java JVM arguments
    /// </summary>
    public string[] JVMArguments { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Additional minecraft arguments
    /// </summary>
    public string[] MinecraftArguments { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Additional class paths
    /// </summary>
    public string[] AdditionalClassPaths { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The executing class path
    /// </summary>
    public string LaunchClassPath { get; set; } = "net.minecraft.client.main.Main";

    /// <summary>
    /// JVM ram settings
    /// </summary>
    public RAMInfo RAM { get; set; } = new();

    /// <summary>
    /// ModLoader settings
    /// </summary>
    public ModLoaderModel ModLoader { get; set; } = new();

    /// <summary>
    /// An array of installed mods
    /// </summary>
    public ModModel[] Mods { get; set; } = Array.Empty<ModModel>();

    /// <summary>
    /// The minecraft version
    /// </summary>
    public MinecraftVersion MinecraftVersion { get; set; }

    /// <summary>
    /// The DateTime the instance was created
    /// </summary>
    public DateTime Created { get; set; } = DateTime.Now;

    /// <summary>
    /// The DateTime the instance was last updated
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.Now;

    /// <summary>
    /// The client jar that executes the game
    /// </summary>
    public string ClientJar { get; set; } = "";

    /// <summary>
    /// The starting window width
    /// </summary>
    public int WindowWidth { get; set; } = 854;

    /// <summary>
    /// The starting window height
    /// </summary>
    public int WindowHeight { get; set; } = 480;

    /// <summary>
    /// The instance manager
    /// </summary>
    [JsonIgnore]
    public InstanceManager InstanceManager { get; set; }

    /// <summary>
    /// An array of class paths.
    /// </summary>
    public string[] ClassPaths { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The source of the instance.
    /// </summary>
    public PlatformSource Source { get; set; } = PlatformSource.Unknown;
}