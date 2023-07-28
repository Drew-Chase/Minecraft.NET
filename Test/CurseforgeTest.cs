/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Curseforge.Controller;
using Chase.Minecraft.Curseforge.Model;
using Chase.Minecraft.Model;
using Serilog;

namespace Test;

internal static class CurseforgeTest
{
    public static async Task Start()
    {
        await Overview();
    }

    private static async Task SearchMod(CurseforgeClient client)
    {
        var project = await client.SearchModsAsync("Warp", "1.19.4", Chase.Minecraft.ModLoaders.Fabric);
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Log.Debug($" Search Curseforge Mod!");
    }

    private static async Task SearchModpack(CurseforgeClient client)
    {
        var project = await client.SearchModpackAsync("Warp", "1.19.4", Chase.Minecraft.ModLoaders.Fabric);
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Log.Debug($" Search Curseforge Modpack!");
    }

    private static async Task SearchResourcepack(CurseforgeClient client)
    {
        var project = await client.SearchResourcepacksAsync("Faithful", "1.19.4");
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Log.Debug($" Search Curseforge Resourcepacks!");
    }

    private static async Task SearchWorlds(CurseforgeClient client)
    {
        var project = await client.SearchWorldsAsync("OneBlock ", "1.19.4");
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Log.Debug($" Search Curseforge World!");
    }

    private static async Task Overview()
    {
        InstanceModel instance = new();
        // Creates a Curseforge Client with an API-Key
        using CurseforgeClient client = new CurseforgeClient("$2a$10$qD2UJdpHaeDaQyGGaGS0QeoDnKq2EC7sX6YSjOxYHtDZSQRg04BCG");

        // Search for projects
        CurseforgeSearchResult? modpacks = await client.SearchModpackAsync("All the Mods 6", "1.16.5", Chase.Minecraft.ModLoaders.Forge);
        CurseforgeSearchResult? mods = await client.SearchModsAsync("Warp", "1.19.4", Chase.Minecraft.ModLoaders.Fabric);
        CurseforgeSearchResult? worlds = await client.SearchWorldsAsync("OneBlock ", "1.19.4");
        CurseforgeSearchResult? resourcepacks = await client.SearchResourcepacksAsync("Faithful", "1.19.4");

        // Get Individual Projects
        CurseforgeProject? mod = await client.GetMod("887168");
        CurseforgeProject? modpack = await client.GetModpack("887168");
        CurseforgeProject? resourcepack = await client.GetResourcepack("887168");
        CurseforgeProject? world = await client.GetWorld("887168");

        // Get Project Files
        ModFile[]? modFiles = await client.GetModFiles("887168");
        ModFile[]? modpackFiles = await client.GetModpackFiles("887168");
        ModFile[]? resourcepackFiles = await client.GetResourcepackFiles("887168");
        ModFile[]? worldFiles = await client.GetWorldFiles("887168");

        // Get Project Files
        ModFile? modFile = await client.GetModFile("887168", "250");
        ModFile? modpackFile = await client.GetModpackFile("887168", "250");
        ModFile? resourcepackFile = await client.GetResourcepackFile("887168", "250");
        ModFile? worldFile = await client.GetWorldFile("887168", "250");

        // Downloads the file
        await client.Download(modFile.Value, instance, "mods");
        await client.Download(modpackFile.Value, "/path/to/modpacks/");
        await client.Download(resourcepackFile.Value, instance, "resourcepack");
        await client.Download(worldFile.Value, instance, "saves");
    }
}