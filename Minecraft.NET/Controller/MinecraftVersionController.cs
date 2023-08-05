/*
    PolygonMC - LFInteractive LLC. 2021-2024
    PolygonMC is a free and open source Minecraft Launcher implementing various modloaders, mod platforms, and minecraft authentication.
    PolygonMC is protected under GNU GENERAL PUBLIC LICENSE version 3.0 License
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
    https://github.com/DcmanProductions/PolygonMC
*/

using Chase.Minecraft.Model;
using Chase.Networking;
using Newtonsoft.Json.Linq;

namespace Chase.Minecraft.Controller;

public static class MinecraftVersionController
{
    /// <summary>
    /// Asynchronously gets a MinecraftVersion object by its name.
    /// </summary>
    /// <param name="name">The name of the Minecraft version to retrieve.</param>
    /// <returns>
    /// A Task that represents the asynchronous operation. The task result contains the retrieved
    /// MinecraftVersion object, if found; otherwise, null.
    /// </returns>
    public static async Task<MinecraftVersion?> GetVersionByNameAsync(string name) => (await GetMinecraftVersionManifestAsync())?.Versions.First(i => i.ID == name);

    /// <summary>
    /// Gets a MinecraftVersion object by its name.
    /// </summary>
    /// <param name="name">The name of the Minecraft version to retrieve.</param>
    /// <returns>The retrieved MinecraftVersion object, if found; otherwise, null.</returns>
    public static MinecraftVersion? GetMinecraftVersionByName(string name) => GetVersionByNameAsync(name).Result;

    /// <summary>
    /// Asynchronously gets the Minecraft version manifest.
    /// </summary>
    /// <returns>
    /// A Task that represents the asynchronous operation. The task result contains the retrieved
    /// MinecraftVersionManifest object, if successful; otherwise, null.
    /// </returns>
    public static async Task<MinecraftVersionManifest?> GetMinecraftVersionManifestAsync()
    {
        using NetworkClient client = new();
        using HttpResponseMessage response = await client.GetAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json");
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content)?.ToObject<MinecraftVersionManifest>();
        }

        return null;
    }

    /// <summary>
    /// Asynchronously gets the latest Minecraft version from the version manifest.
    /// </summary>
    /// <returns>
    /// A Task that represents the asynchronous operation. The task result contains the latest
    /// MinecraftVersion object, if successful; otherwise, null.
    /// </returns>
    public static async Task<MinecraftVersion?> GetLatestMinecraftVersionAsync()
    {
        var manifest = await GetMinecraftVersionManifestAsync();
        if (manifest != null && manifest.HasValue)
        {
            return manifest.Value.Versions.FirstOrDefault(i => i.ID == manifest?.Latest.Release);
        }
        return null;
    }

    /// <summary>
    /// Gets the Minecraft version manifest.
    /// </summary>
    /// <returns>The retrieved MinecraftVersionManifest object, if successful; otherwise, null.</returns>
    public static MinecraftVersionManifest? GetMinecraftVersionManifest() => GetMinecraftVersionManifestAsync().Result;
}