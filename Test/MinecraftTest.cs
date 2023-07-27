/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Controller;
using Chase.Minecraft.Instances;
using Chase.Minecraft.Model;
using System.Diagnostics;

namespace Test;

internal static class MinecraftTest
{
    public static async Task Start()
    {
    }

    private static async Task DownloadJava()
    {
        await JavaController.DownloadJava("./java"); // This will download Java 8 (Legacy) and Java 17 (Latest) from Mojang and place it in the directory specified.

        string[] installedJavaVersions = JavaController.GetGlobalJVMInstallations(); // This will get all java versions that are installed on the system and available through the systems Environment 'PATH'

        JVMInstallations installations = JavaController.GetLocalJVMInstallations("./java"); // This will get the installed Legacy and Latest java versions or null if not installed.
    }

    private static async Task LaunchMinecraft()
    {
        // Create an instance manager This will handle all instances for the directory specified.
        InstanceManager manager = new InstanceManager("./instances");

        // Create an instance with the name of test. This will be placed in the instance directory
        // under a new directory named the same as the instance name. If a instance already exists a
        // (1) will be appended to the end of it. If the name contains invalid characters they will
        // be replaced with a '-'
        InstanceModel instance = new InstanceModel()
        {
            Name = "Test",
            Description = "This is a test instance",
            Java = JavaController.GetLocalJVMInstallations("./java").Latest, // This is the path to the java.exe, you can manually specify a locally installed java version or use the built in JavaController.
            WindowWidth = 1280,
            WindowHeight = 720,
            RAM = new RAMInfo()
            {
                MaximumRamMB = 4096,
                MinimumRamMB = 1024
            }
        };
        instance = manager.Create(instance); // This also returns the instance that was created with additional information.
        instance = manager.GetFirstInstancesByName("Test"); // Gets the first instance found with the name of "Test"
        InstanceModel[] instances = manager.GetInstancesByName("Test"); // Gets a list of instances with the name of "Test"
        instance = manager.GetInstanceById(instance.Id); // Gets the instance based on the unique GUID

        using MinecraftClient client = new MinecraftClient("dev", "./minecraft", instance);  // Creates a minecraft client based on the instance with an offline user

        // Setup client information.
        client.SetClientInfo("Azure Client ID", "Azure Client Name", "Client Version"); // This is required to authenticate the user. view https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps
        await client.AuthenticateUser(); // This will prompt the user to login to their Microsoft Account.

        // Downloads the clients resources. This is done automatically when you run the client. or
        // you can do it manually.
        await client.DownloadLibraries(); // This downloads the clients libraries
        await client.DownloadAssets(); // This will download any assets needed for minecraft.
        await client.DownloadClient(); // This will download the client jar.

        DataReceivedEventHandler outputHandler = (s, e) =>
        {
            string? data = e.Data;
            if (!string.IsNullOrWhiteSpace(data))
            {
                Console.WriteLine(data); // This will write each line from Minecraft to the console.
            }
        };
        Process process = client.Start(); // this will start the minecraft client based on the information previously provided.
        process.WaitForExit();
    }
}