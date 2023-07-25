// LFInteractive LLC. 2021-2024﻿

using System.Diagnostics;

namespace Test;

internal class Program
{
    private static async Task Main()
    {
        Console.Write("Test What?\n0 = Modrinth\n1 = CurseForge\n2 = Minecraft\nIndex: ");
        var watch = Stopwatch.StartNew();
        string input = Console.ReadLine();
        switch (input)
        {
            case "0":
                await ModrinthTest.Start();
                break;

            case "1":
                await CurseforgeTest.Start();
                break;

            case "2":
                await MinecraftTest.Start();
                break;
        }
        await Console.Out.WriteLineAsync($"Process took: {watch.Elapsed}");
        Console.ReadLine();
    }
}