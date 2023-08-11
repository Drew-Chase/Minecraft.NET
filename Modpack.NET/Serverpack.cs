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
        string javaExe = "";
        string[] exes = JavaController.GetGlobalJVMInstallations();
        if (exes.Any())
        {
            javaExe = exes[0];
        }
        return await TryGenerateServerpack(instance, archivePath, javaExe);
    }

    public static async Task<bool> TryGenerateServerpack(InstanceModel instance, string archivePath, string javaExe) =>
        await TryGenerateServerpack(instance.ModLoader, instance.MinecraftVersion, Path.Combine(instance.Path, "mods"), archivePath, javaExe);

    public static async Task<bool> TryGenerateServerpack(ModLoaderModel loader, MinecraftVersion version, string modsDirectory, string archivePath, string javaExe)
    {
        try
        {
            string tmp = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "LFInteractive", "Minecraft.NET", "serverpacks", Path.GetRandomFileName())).FullName;
            string modsDir = Directory.CreateDirectory(Path.Combine(tmp, "mods")).FullName;
            string[] serverMods = GetServerMods(modsDirectory, loader.Modloader, out string[] additionalFiles);
            foreach (string file in serverMods)
            {
                File.Copy(file, Path.Combine(modsDir, Path.GetFileName(file)), true);
            }
            foreach (string file in additionalFiles)
            {
                File.Copy(file, Path.Combine(modsDir, Path.GetFileName(file)), true);
            }

            using (NetworkClient client = new())
            {
                switch (loader.Modloader)
                {
                    case ModLoaders.None:
                        PistonModel? piston = await MinecraftVersionController.GetPistonData(version);
                        if (piston == null)
                        {
                            return false;
                        }
                        string url = piston.Value.Downloads.Server.Url;
                        await client.DownloadFileAsync(url, Path.Combine(tmp, "server.jar"));
                        break;

                    case ModLoaders.Fabric:
                        await FabricLoader.InstallServer(tmp, loader.Version, version);
                        break;

                    case ModLoaders.Forge:
                        if (!File.Exists(javaExe))
                        {
                            return false;
                        }
                        await ForgeLoader.InstallServer(tmp, loader.Version, version, javaExe, (s, e) =>
                        {
                            string? data = e.Data;
                            if (!string.IsNullOrWhiteSpace(data))
                            {
                                Log.Debug(data);
                            }
                        });
                        break;
                }
            }
            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
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

    public static string[] GetServerMods(string modsPath, ModLoaders loader) => GetServerMods(modsPath, loader, out _);

    public static string[] GetServerMods(string modsPath, ModLoaders loader, out string[] nonModdedFiles)
    {
        Log.Information("Getting '{LOADER}' server mods from directory: {PATH}", loader, modsPath);
        List<string> mods = new();
        List<string> nonModdedList = new();
        string[] files = Directory.GetFiles(modsPath, "*.jar", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            Log.Debug("Scanning '{FILE}'", Path.GetFileName(file));
            bool found = false;
            try
            {
                using ZipArchive archive = ZipFile.OpenRead(file);
                switch (loader)
                {
                    case ModLoaders.Fabric:
                        FabricModJson? json = FabricLoader.GetLoaderFile(archive);
                        if (json != null && json.Value.Environment != Fabric.Environment.Client)
                        {
                            found = true;
                            if ((json.Value.EntryPoints.Main != null && json.Value.EntryPoints.Main.Any()) || (json.Value.EntryPoints.Server != null && json.Value.EntryPoints.Server.Any()))
                            {
                                Log.Debug("File was a server mod '{FILE}'", Path.GetFileName(file));
                                mods.Add(file);
                            }
                        }
                        break;

                    case ModLoaders.Forge:
                        ModToml? toml = ForgeLoader.GetLoaderFile(archive);
                        try
                        {
                            if (toml != null)
                            {
                                found = true;
                                if (toml.Value.Dependencies.Any(i => i.ModId == "forge"))
                                {
                                    Side side = toml.Value.Dependencies.First(i => i.ModId == "forge").Side;
                                    if (side != Side.CLIENT)
                                    {
                                        Log.Debug("File was a server mod '{FILE}'", Path.GetFileName(file));
                                        mods.Add(file);
                                    }
                                }
                                else
                                {
                                    mods.Add(file);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error("Unable to parse mod.toml from jar: {JAR} - {MSG}", file, e.Message, e);
                        }
                        break;
                }
                if (!found)
                {
                    nonModdedList.Add(file);
                }
            }
            catch (Exception e)
            {
                Log.Error("Unable to verify mod: {FILE} - {MSG}", Path.GetFileName(file), e.Message, e);
                continue;
            }
        }
        nonModdedFiles = nonModdedList.ToArray();
        return mods.ToArray();
    }
}