// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Controller;
using System.Diagnostics;

namespace Test;

internal static class MinecraftTest
{
    public static async Task Start()
    {
        var watch = Stopwatch.StartNew();
        using MinecraftClient client = new(new() { Directory = "./", JavaExecutable = JavaController.GetLocalJVMInstallations()[0], Username = "dcman58" }, "f8b88f7d-77d7-49ca-9b97-5bb12a4ee48f", "Better Minecraft Launcher", "0.0.1");
        await client.AuthenticateUser();
        client.SetMinecraftVersion("1.20.1");
        var process = client.Start();
        process.WaitForExit();
        await Console.Out.WriteLineAsync($"Process took: {watch.Elapsed}");
        Console.ReadLine();
    }

    private static async Task DownloadJava()
    {
        await Console.Out.WriteLineAsync("Downloading Java");
        await JavaController.DownloadJava("java");
    }
}