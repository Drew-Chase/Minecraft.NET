// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Controller;

namespace Test;

internal static class MinecraftTest
{
    public static async Task Start()
    {
        MinecraftClient minecraftClient = new MinecraftClient(new(){Directory = "", JVMArguments = "", RAM = new() {} });
        //await minecraftClient.DownloadAllLibs(new Uri("https://piston-meta.mojang.com/v1/packages/715ccf3330885e75b205124f09f8712542cbe7e0/1.20.1.json"));
        await minecraftClient.DownloadAllAssets(new Uri("https://piston-meta.mojang.com/v1/packages/9d58fdd2538c6877fb5c5c558ebc60ee0b6d0e84/5.json"));
        Console.WriteLine("Done");
        Console.ReadLine();
    }
}