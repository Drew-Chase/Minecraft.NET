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
using Chase.Minecraft.Modrinth.Controller;
using Chase.Minecraft.Modrinth.Model;
using Chase.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.IO.Compression;
using System.Text.RegularExpressions;

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

    public static async Task<InstanceModel> ImportModrinth(string file, InstanceManager manager, string javaPath, string rootDirectory, string curseforge_api)
    {
        ModrinthModpackModel modpackModel = new();
        string overloadsDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "LFInteractive", "Minecraft.NET", Path.GetRandomFileName())).FullName;
        using (ZipArchive archive = ZipFile.OpenRead(file))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.Name.Equals("modrinth.index.json", StringComparison.CurrentCultureIgnoreCase))
                {
                    StreamReader reader = new(entry.Open());
                    modpackModel = JObject.Parse(reader.ReadToEnd()).ToObject<ModrinthModpackModel>();
                    break;
                }
            }
            archive.ExtractToDirectory(overloadsDirectory);
            overloadsDirectory = Path.Combine(overloadsDirectory, "overrides");
        }

        MinecraftVersion? minecraftVersion = MinecraftVersionController.GetMinecraftVersionByName(modpackModel.Dependencies.Minecraft);
        if (minecraftVersion != null)
        {
            InstanceModel instance = manager.Create(new()
            {
                Name = modpackModel.Name,
                Description = modpackModel.Summary,
                GameVersion = modpackModel.Dependencies.Minecraft,
                ModLoader = new()
                {
                    Modloader = !string.IsNullOrWhiteSpace(modpackModel.Dependencies.ForgeLoader) ? ModLoaders.Forge : !string.IsNullOrWhiteSpace(modpackModel.Dependencies.FabricLoader) ? ModLoaders.Fabric : ModLoaders.None,
                    Version = !string.IsNullOrWhiteSpace(modpackModel.Dependencies.ForgeLoader) ? modpackModel.Dependencies.ForgeLoader : !string.IsNullOrWhiteSpace(modpackModel.Dependencies.FabricLoader) ? modpackModel.Dependencies.FabricLoader : "",
                },
                MinecraftVersion = minecraftVersion.Value,
                Java = JavaController.GetValidJavaInstallationForMinecraftVersion(minecraftVersion.Value, javaPath),
                Source = Data.PlatformSource.Modrinth,
                JVMArguments = Array.Empty<string>(),
                MinecraftArguments = Array.Empty<string>(),
                AdditionalClassPaths = Array.Empty<string>(),
                ClassPaths = Array.Empty<string>(),
                RAM = new(),
                Mods = Array.Empty<ModModel>(),
            });


            foreach (string entry in Directory.GetFileSystemEntries(overloadsDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                FileInfo info = new(entry);
                if (info.Attributes.HasFlag(FileAttributes.Directory))
                {
                    Directory.Move(entry, Path.Combine(instance.Path, info.Name));
                }
                else
                {
                    File.Move(entry, Path.Combine(instance.Path, info.Name));
                }
            }

            List<ModModel> mods = new();
            using (NetworkClient networkClient = new())
            {
                foreach (ModpackFile item in modpackModel.Files)
                {
                    if (item.Path.StartsWith("mods/"))
                    {
                        string url = item.Downloads.First();
                        string projectId = "";
                        string versionId = "";
                        string filename = "";

                        Match match = Regex.Match(url, @"/data/([^/]+)/versions/([^/]+)/([^/]+\.jar)");

                        if (match.Success)
                        {
                            projectId = match.Groups[1].Value;
                            versionId = match.Groups[2].Value;
                            filename = match.Groups[3].Value;
                        }

                        mods.Add(new()
                        {
                            Name = filename,
                            FileName = filename,
                            ProjectID = projectId,
                            VersionID = versionId,
                            DownloadURL = url,
                            Source = Data.PlatformSource.Modrinth,
                        });
                    }
                    else
                    {
                        string path = Path.Combine(instance.Path, item.Path);
                        string directory = Directory.CreateDirectory(Directory.GetParent(path).FullName).FullName;
                        await networkClient.DownloadFileAsync(item.Downloads.First(), path);
                    }
                }
            }

            instance.Mods = mods.ToArray();

            InstanceManager.ReDownloadMods(instance);

            using (MinecraftClient client = new("", rootDirectory, instance))
            {
                Task.WaitAll(client.DownloadClient(), client.DownloadLibraries(), client.DownloadAssets());
            }

            if (instance.ModLoader.Modloader == ModLoaders.Fabric)
            {
                await FabricLoader.Install(instance.ModLoader.Version, instance);
            }
            else if (instance.ModLoader.Modloader == ModLoaders.Forge)
            {
                await ForgeLoader.Install(instance.ModLoader.Version, instance, rootDirectory, JavaController.GetLocalJVMInstallations(javaPath).Latest);
            }

            using (ModrinthClient client = new())
            {
                Log.Information("Mapped {COUNT} unmapped mods", ModpackUtils.MapUnMappedMods(instance, client, new Curseforge.Controller.CurseforgeClient(curseforge_api)));
            }

            return manager.Save(instance.Id, instance);
        }
        throw new Exception($"Unable to parse minecraft version, {modpackModel.Dependencies.Minecraft}");

    }

}
