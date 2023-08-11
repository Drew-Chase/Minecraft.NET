/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Model;
using Chase.Minecraft.Model.Piston;
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

    public static async Task<PistonModel?> GetPistonData(MinecraftVersion version)
    {
        using NetworkClient client = new();
        return (await client.GetAsJson(version.URL.ToString()))?.ToObject<PistonModel>();
    }
}