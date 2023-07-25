// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Controller;
using System.Diagnostics;

namespace Test;

internal static class MinecraftTest
{
    public static async Task Start()
    {
        var watch = Stopwatch.StartNew();
        await MicrosoftAuthenticationController.LogIn("f8b88f7d-77d7-49ca-9b97-5bb12a4ee48f");
        //ClientStartInfo info = new() { Username = "dcman58", Directory = Directory.GetParent(Process.GetCurrentProcess().MainModule?.FileName ?? "")?.FullName ?? Environment.CurrentDirectory, JavaExecutable = Path.Combine(JavaController.GetLocalJVMInstallations()[0], "java.exe") };
        //using MinecraftClient client = new(info);
        //MinecraftClient.Launch(info, "1.20.1", (s, e) =>
        //{
        //    string? data = e.Data;
        //    if (data != null)
        //    {
        //        Console.WriteLine(data);
        //    }
        //}).WaitForExit();
        Console.WriteLine("Done");
        await Console.Out.WriteLineAsync($"Process took: {watch.Elapsed}");
        Console.ReadLine();
    }

    private static async Task DownloadJava()
    {
        await Console.Out.WriteLineAsync("Downloading Java");
        await JavaController.DownloadJava("java");
    }
}