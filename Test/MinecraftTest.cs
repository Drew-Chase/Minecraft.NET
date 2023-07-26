/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Controller;
using Chase.Minecraft.Instances;
using Chase.Minecraft.Model;

namespace Test;

internal static class MinecraftTest
{
    public static async Task Start()
    {
        await TheFullMonty();
    }

    private static async Task TheFullMonty()
    {
        string javaPath = Path.GetFullPath("./java");
        await JavaController.DownloadJava(javaPath);
        InstanceManager manager = new(Path.GetFullPath("./minecraft/instances"));
        InstanceModel? instance = null;
        if (!manager.Exist("Test"))
        {
            MinecraftVersion? version = MinecraftVersionController.GetMinecraftVersionByName("1.20.1");
            if (version != null)
            {
                instance = manager.Create(new()
                {
                    Name = "Test",
                    Java = JavaController.GetLocalJVMInstallations(javaPath).Latest,
                    MinecraftVersion = version.Value
                });
            }
        }
        else
        {
            instance = manager.GetFirstInstancesByName("Test");
        }
        if (instance != null)
        {
            MinecraftClient client = new("dcman58", Path.GetFullPath("./minecraft"), manager, instance);
            client.SetClientInfo("f8b88f7d-77d7-49ca-9b97-5bb12a4ee48f", "PolygonMC", "0.0.0");
            await client.AuthenticateUser();
            var process = MinecraftClient.Launch("dcman58", Path.GetFullPath("./minecraft"), manager, instance);
            process.WaitForExit();
        }
    }
}