// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Curseforge.Controller;

namespace Test;

internal static class CurseforgeTest
{
    public static async Task Start()
    {
        using CurseforgeClient client = new("$2a$10$qD2UJdpHaeDaQyGGaGS0QeoDnKq2EC7sX6YSjOxYHtDZSQRg04BCG");
        await Search(client);
    }

    private static async Task Search(CurseforgeClient client)
    {
        var project = await client.SearchModsAsync("Warp", "1.19.4", Chase.Minecraft.ModLoaders.Fabric);
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Search Curseforge Mod!");
    }
}