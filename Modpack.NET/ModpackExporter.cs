/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Model;
using Serilog;
using System.IO.Compression;

namespace Chase.Minecraft.Modpacks;

public static class ModpackExporter
{
    public static bool Export(string path, InstanceModel instance, params string[] additionalPaths)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);

            using ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(Path.Combine(instance.Path, "instance.json"), "instance.json");
            CompressionLevel level = CompressionLevel.SmallestSize;
            foreach (string mod in GetUnknownMods(instance))
            {
                Log.Debug("[EXPORT] Archiving {FILE}", mod);
                string relativePath = Path.GetRelativePath(instance.Path, mod);
                archive.CreateEntryFromFile(mod, relativePath, level);
            }

            foreach (string additionalPath in additionalPaths)
            {
                string[] files = Directory.GetFiles(additionalPath, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    Log.Debug("[EXPORT] Archiving {FILE}", file);
                    string relativePath = Path.GetRelativePath(instance.Path, file);
                    archive.CreateEntryFromFile(file, relativePath, level);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Log.Error("Unable to export modpack \"{INS}\"", instance.Name, e);
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                Log.Error("Unable to remove archive: {MSG}", ex.Message, ex);
            }
        }

        return false;
    }

    public static string[] GetUnknownMods(InstanceModel instance)
    {
        IEnumerable<string> mods = instance.Mods.Select(i => i.Name);
        string[] modJars = Directory.GetFiles(Path.Combine(instance.Path, "mods"), "*.jar", SearchOption.AllDirectories);
        List<string> unknownMods = new();
        foreach (string mod in modJars)
        {
            string name = Path.GetFileName(mod);
            if (!mods.Contains(name))
            {
                unknownMods.Add(mod);
            }
        }

        return unknownMods.ToArray();
    }
}