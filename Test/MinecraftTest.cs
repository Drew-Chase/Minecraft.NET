// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Controller;

namespace Test;

internal static class MinecraftTest
{
    public static async Task Start()
    {
        //        Process.Start("explorer.exe");
        MinecraftClient minecraftClient = new MinecraftClient(new(){Directory = "", JVMArguments = "", RAM = new() {} });
        await minecraftClient.DownloadAllLibs();
        Console.ReadLine();
    }
}