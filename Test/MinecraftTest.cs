/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Controller;
using Chase.Minecraft.Fabric;
using Chase.Minecraft.Forge;
using Chase.Minecraft.Instances;
using Chase.Minecraft.Model;
using Serilog;
using System.Diagnostics;

namespace Test;

internal static class MinecraftTest
{
    public static async Task Start()
    {
        try
        {
            await DownloadJava();
            await LaunchMinecraft();
        }
        catch (Exception ex)
        {
            Log.Error("Failed to run test", ex);
        }
    }

    private static async Task DownloadJava()
    {
        await JavaController.DownloadJava("./java"); // This will download Java 8 (Legacy) and Java 17 (Latest) from Mojang and place it in the directory specified.
        Log.Debug("Java Done");
        string[] installedJavaVersions = JavaController.GetGlobalJVMInstallations(); // This will get all java versions that are installed on the system and available through the systems Environment 'PATH'
        Log.Debug("Java Global");

        JVMInstallations installations = JavaController.GetLocalJVMInstallations("./java"); // This will get the installed Legacy and Latest java versions or null if not installed.
        Log.Debug("Java Local");
    }

    private static async Task LaunchMinecraft()
    {
        // Create an instance manager This will handle all instances for the directory specified.
        InstanceManager manager = new InstanceManager("./minecraft/instances");

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
        if (!manager.Exist("Test")) // Checks if the instance exists
        {
            instance = manager.Create(instance); // This also returns the instance that was created with additional information.
            InstanceModel[] instances = manager.GetInstancesByName("Test"); // Gets a list of instances with the name of "Test"
            instance = manager.GetInstanceById(instance.Id); // Gets the instance based on the unique GUID
        }
        instance = manager.GetFirstInstancesByName("Test"); // Gets the first instance found with the name of "Test"

        MinecraftVersionManifest minecraftVersionManifest = MinecraftVersionController.GetMinecraftVersionManifest().Value; // Gets a minecraft version manifest from mojang
        MinecraftVersion[] versions = minecraftVersionManifest.Versions; // Gets a list of all minecraft versions, releases and snapshots

        string latestSnapshot = minecraftVersionManifest.Latest.Snapshot; // The latest Minecraft Snapshot Version as a string
        string latestMinecraftVerson = minecraftVersionManifest.Latest.Release; // The latest Minecraft Version as a string

        MinecraftVersion latestVersion = MinecraftVersionController.GetMinecraftVersionByName(latestMinecraftVerson).Value; // Creates a MinecraftVersion object based on the version string
        instance.MinecraftVersion = latestVersion; // This sets the minecraft version to the latest
        manager.Save(instance.Id, instance); // This saves the instance to file
        instance.InstanceManager.Save(instance.Id, instance); // This gets the instance manager from the instance and saves it to file.

        using MinecraftClient client = new MinecraftClient("dcman58", "./minecraft", instance);  // Creates a minecraft client based on the instance with an offline user

        // Setup client information.
        client.SetClientInfo("f8b88f7d-77d7-49ca-9b97-5bb12a4ee48f", "Azure Client Name", "0.0.0", new("http://127.0.0.1:56748")); // This is required to authenticate the user. view https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps
        await client.AuthenticateUser();

        // Downloads the clients resources. This is done automatically when you run the client. or
        // you can do it manually.
        await client.DownloadLibraries(); // This downloads the clients libraries
        await client.DownloadAssets(); // This will download any assets needed for minecraft.
        await client.DownloadClient(); // This will download the client jar.

        // Gets the latest loader and installs it
        string[] loader_versions = await ForgeLoader.GetLoaderVersions("1.20.1"); // This gets an array of all fabric loader versions
        instance = await ForgeLoader.Install(loader_versions.First(), instance) ?? instance; // Downloads and installs the specified fabric loader version to the specified instance

        DataReceivedEventHandler outputHandler = (s, e) =>
        {
            string? data = e.Data;
            if (!string.IsNullOrWhiteSpace(data))
            {
                Console.WriteLine(data); // This will write each line from Minecraft to the console.
            }
        };
        Process process = client.Start(outputHandler); // this will start the minecraft client based on the information previously provided.
        process.WaitForExit();
    }

    private static async Task InstallFabric(InstanceModel instance)
    {
        // Gets the latest loader and installs it
        string[] loader_versions = await FabricLoader.GetLoaderVersions(); // This gets an array of all fabric loader versions
        await FabricLoader.Install(loader_versions.First(), instance); // Downloads and installs the specified fabric loader version to the specified instance
    }

    private static async Task InstallForge(InstanceModel instance)
    {
        // Gets the latest loader and installs it
        string[] loader_versions = await ForgeLoader.GetLoaderVersions("1.20.1"); // This gets an array of all fabric loader versions
        await ForgeLoader.Install(loader_versions.First(), instance); // Downloads and installs the specified fabric loader version to the specified instance
    }
}