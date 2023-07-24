// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Controller;
using Chase.Minecraft.Model;
using System.Diagnostics;

namespace Test;

internal static class MinecraftTest
{
    public static async Task Start()
    {
        var watch = Stopwatch.StartNew();
        using MinecraftClient client = new(new() { Directory = "", JVMArguments = "", RAM = new() { } });
        MinecraftVersion? version = await client.GetLatestMinecraftVersionAsync();
        if (version != null)
        {
            Task.WaitAll(DownloadLibraries(client, version.Value), DownloadAssets(client, version.Value));
        }
        Console.WriteLine("Done");
        await Console.Out.WriteLineAsync($"Process took: {watch.Elapsed}");
        Console.ReadLine();
    }

    private static async Task DownloadLibraries(MinecraftClient client, MinecraftVersion version)
    {
        await Console.Out.WriteLineAsync("Downloading Libraries");
        await client.DownloadLibraries(version);
    }

    private static async Task DownloadAssets(MinecraftClient client, MinecraftVersion version)
    {
        await Console.Out.WriteLineAsync("Downloading Assets");
        await client.DownloadAssets(version);
    }
}