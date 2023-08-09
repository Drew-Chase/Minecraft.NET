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
using Newtonsoft.Json.Linq;
using Serilog;
using System.IO.Compression;

namespace Chase.Minecraft.Modpacks;

public static class ModpackImporter
{
    public static async Task<InstanceModel> Import(string path, string name, string javaExecutable, string rootDirectory, InstanceManager manager)
    {
        InstanceModel instance = new();
        if (!File.Exists(path))
        {
            return instance;
        }
        Log.Information("Importing Minecraft Instance: {NAME}", name);
        string tempDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "LFInteractive", "Minecraft.NET", "Imports", Path.GetRandomFileName())).FullName;

        using (ZipArchive archive = ZipFile.OpenRead(path))
        {
            archive.ExtractToDirectory(tempDirectory);
        }

        InstanceModel exportedInstance = JObject.Parse(File.ReadAllText(Path.Combine(tempDirectory, "instance.json"))).ToObject<InstanceModel>() ?? new();
        if (!string.IsNullOrWhiteSpace(exportedInstance.Name))
        {
            exportedInstance.Id = Guid.NewGuid();
            exportedInstance.Name = name;
            exportedInstance.Java = javaExecutable;
            exportedInstance.AdditionalClassPaths = Array.Empty<string>();
            exportedInstance.ClassPaths = Array.Empty<string>();

            InstanceModel importedInstance = manager.Create(exportedInstance);

            using (MinecraftClient minecraftClient = new("", rootDirectory, importedInstance))
            {
                Log.Information("Downloading Minecraft Client...");
                Task.WaitAll(new Task[]
                {
                    minecraftClient.DownloadLibraries(),
                    minecraftClient.DownloadAssets(),
                    minecraftClient.DownloadClient(),
                });
            }

            switch (importedInstance.ModLoader.Modloader)
            {
                case ModLoaders.Fabric:
                    Log.Information("Installing Fabric...");
                    await FabricLoader.Install(importedInstance.ModLoader.Version, importedInstance);
                    break;

                case ModLoaders.Forge:
                    Log.Information("Installing Forge...");
                    InstanceModel? tmp = await ForgeLoader.Install(importedInstance.ModLoader.Version, importedInstance, rootDirectory, javaExecutable);
                    if (tmp != null)
                    {
                        importedInstance = tmp;
                    }
                    break;
            }

            Log.Information("Downloading Mods...");
            InstanceManager.ReDownloadMods(importedInstance);
            string[] overrides = Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories);

            foreach (string item in overrides)
            {
                string relative = Path.GetRelativePath(tempDirectory, item);
                string newPath = Path.Combine(importedInstance.Path, relative);

                DirectoryInfo? parent = Directory.GetParent(newPath);
                if (parent != null && !parent.Exists)
                {
                    parent.Create();
                }
                File.Move(item, newPath, true);
            }

            manager.Save(importedInstance.Id, importedInstance);
        }

        return instance;
    }
}