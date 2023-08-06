/*
    PolygonMC - LFInteractive LLC. 2021-2024
    PolygonMC is a free and open source Minecraft Launcher implementing various modloaders, mod platforms, and minecraft authentication.
    PolygonMC is protected under GNU GENERAL PUBLIC LICENSE version 3.0 License
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
    https://github.com/DcmanProductions/PolygonMC
*/

namespace Chase.Minecraft.Model;

public sealed class RAMInfo
{
    public int MaximumRamMB { get; set; } = 4096;

    public int MinimumRamMB { get; set; } = 4096;
}