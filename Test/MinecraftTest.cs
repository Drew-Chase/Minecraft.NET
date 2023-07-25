/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Controller;

namespace Test;

internal static class MinecraftTest
{
    public static async Task Start()
    {
        using MinecraftClient client = new(new() { Directory = "./", JavaExecutable = JavaController.GetLocalJVMInstallations()[0], Username = "dcman58" }, "f8b88f7d-77d7-49ca-9b97-5bb12a4ee48f", "Better Minecraft Launcher", "0.0.1");
        await client.AuthenticateUser();
        client.SetMinecraftVersion("1.20.1");
        var process = client.Start();
        process.WaitForExit();
    }

    private static async Task DownloadJava()
    {
        await Console.Out.WriteLineAsync("Downloading Java");
        await JavaController.DownloadJava("java");
    }
}