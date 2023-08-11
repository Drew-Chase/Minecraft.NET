/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Controller;
using Chase.Minecraft.Fabric;
using Chase.Minecraft.Forge;
using Chase.Minecraft.Forge.Model;
using Chase.Minecraft.Model;
using Chase.Minecraft.Model.Piston;
using Chase.Networking;
using Serilog;
using System.IO.Compression;

namespace Chase.Minecraft.Modpacks;

public static class Serverpack
{
    public static async Task<bool> TryGenerateServerpack(InstanceModel instance, string archivePath)
    {
        try
        {
            string tmp = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "LFInteractive", "Minecraft.NET", "serverpacks", Path.GetTempFileName())).FullName;
            string modsDir = Directory.CreateDirectory(Path.Combine(tmp, "mods")).FullName;
            foreach (string file in GetServerMods(Path.Combine(instance.Path, "mods"), instance.ModLoader.Modloader))
            {
                File.Copy(file, Path.Combine(modsDir, Path.GetFileName(file)), true);
            }

            using (NetworkClient client = new())
            {
                switch (instance.ModLoader.Modloader)
                {
                    case ModLoaders.None:
                        PistonModel? piston = await MinecraftVersionController.GetPistonData(instance.MinecraftVersion);
                        if (piston == null)
                        {
                            return false;
                        }
                        string url = piston.Value.Downloads.Server.Url;
                        await client.DownloadFileAsync(url, Path.Combine(tmp, "server.jar"));
                        break;

                    case ModLoaders.Fabric:
                        await FabricLoader.InstallServer(tmp, instance.ModLoader.Version, instance.MinecraftVersion);
                        break;

                    case ModLoaders.Forge:
                        await ForgeLoader.InstallServer(tmp, instance.ModLoader.Version, instance.MinecraftVersion);
                        break;
                }
            }

            ZipFile.CreateFromDirectory(tmp, archivePath);
            Directory.Delete(tmp, true);
            return true;
        }
        catch (Exception e)
        {
            Log.Error("Unable to generate server pack at '{PATH}' - {MSG}", archivePath, e.Message, e);
            return false;
        }
    }

    public static string[] GetServerMods(string modsPath, ModLoaders loader)
    {
        List<string> mods = new();
        string[] files = Directory.GetFiles(modsPath, "*.jar", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            using ZipArchive archive = ZipFile.OpenRead(file);
            switch (loader)
            {
                case ModLoaders.Fabric:
                    FabricModJson? json = FabricLoader.GetLoaderFile(archive);
                    if (json != null && json.Value.Environment != Fabric.Environment.Client)
                    {
                        if (json.Value.EntryPoints.Main.Any() || json.Value.EntryPoints.Server.Any())
                        {
                            mods.Add(file);
                        }
                    }
                    break;

                case ModLoaders.Forge:
                    ModToml? toml = ForgeLoader.GetLoaderFile(archive);
                    if (toml != null)
                    {
                        Side side = toml.Value.Dependencies.First(i => i.ModId == "forge").Side;
                        if (side != Side.CLIENT)
                        {
                            mods.Add(file);
                        }
                    }
                    break;
            }
        }

        return mods.ToArray();
    }
}