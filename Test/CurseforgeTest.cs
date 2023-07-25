/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Curseforge.Controller;

namespace Test;

internal static class CurseforgeTest
{
    public static async Task Start()
    {
        using CurseforgeClient client = new("$2a$10$qD2UJdpHaeDaQyGGaGS0QeoDnKq2EC7sX6YSjOxYHtDZSQRg04BCG");
        await SearchMod(client);
        await SearchModpack(client);
        await SearchResourcepack(client);
        await SearchWorlds(client);
    }

    private static async Task SearchMod(CurseforgeClient client)
    {
        var project = await client.SearchModsAsync("Warp", "1.19.4", Chase.Minecraft.ModLoaders.Fabric);
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Search Curseforge Mod!");
    }

    private static async Task SearchModpack(CurseforgeClient client)
    {
        var project = await client.SearchModpackAsync("Warp", "1.19.4", Chase.Minecraft.ModLoaders.Fabric);
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Search Curseforge Modpack!");
    }

    private static async Task SearchResourcepack(CurseforgeClient client)
    {
        var project = await client.SearchResourcepacksAsync("Faithful", "1.19.4");
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Search Curseforge Resourcepacks!");
    }

    private static async Task SearchWorlds(CurseforgeClient client)
    {
        var project = await client.SearchWorldsAsync("OneBlock ", "1.19.4");
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Search Curseforge World!");
    }
}