// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Controller;

namespace Test;

internal static class MinecraftTest
{
    public static async Task Start()
    {
        using MinecraftClient client = new(new() { Directory = "", JVMArguments = "", RAM = new() { } });
        Task.WaitAll(DownloadLibraries(client), DownloadAssets(client));
        Console.WriteLine("Done");
        Console.ReadLine();
    }

    private static async Task DownloadLibraries(MinecraftClient client)
    {
        await Console.Out.WriteLineAsync("Downloading Libraries");
        await client.DownloadLibraries(new Uri("https://piston-meta.mojang.com/v1/packages/715ccf3330885e75b205124f09f8712542cbe7e0/1.20.1.json"));
    }

    private static async Task DownloadAssets(MinecraftClient client)
    {
        await client.DownloadAssets(new Uri("https://piston-meta.mojang.com/v1/packages/9d58fdd2538c6877fb5c5c558ebc60ee0b6d0e84/5.json"));
    }
}