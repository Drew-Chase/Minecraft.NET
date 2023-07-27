/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft;
using Serilog;
using System.Diagnostics;

namespace Test;

internal class Program
{
    private static async Task Main()
    {
        MinecraftInstance.Initialize();
        Console.Write("Test What?\n0 = Modrinth\n1 = CurseForge\n2 = Minecraft\nIndex: ");
        var watch = Stopwatch.StartNew();
        string input = Console.ReadLine() ?? "";
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
        Log.Debug($"Process took: {watch.Elapsed}");
        //Console.ReadLine();
    }
}