/*
    PolygonMC - LFInteractive LLC. 2021-2024
    PolygonMC is a free and open source Minecraft Launcher implementing various modloaders, mod platforms, and minecraft authentication.
    PolygonMC is protected under GNU GENERAL PUBLIC LICENSE version 3.0 License
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
    https://github.com/DcmanProductions/PolygonMC
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Model;

/// <summary>
/// A data set for Minimum and Maximum Ram
/// </summary>
public sealed class RAMInfo
{
    /// <summary>
    /// Maximum ram allocation in megabytes
    /// </summary>
    [JsonProperty("max-ram")]
    public int Maximum { get; set; } = 4096;

    /// <summary>
    /// Minimum ram allocation in megabytes
    /// </summary>
    [JsonProperty("min-ram")]
    public int Minimum { get; set; } = 256;
}