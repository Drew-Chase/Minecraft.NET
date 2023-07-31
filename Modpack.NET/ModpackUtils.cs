/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Fabric;
using Chase.Minecraft.Forge;
using Chase.Minecraft.Forge.Model;
using Chase.Minecraft.Model;

namespace Chase.Minecraft.Modpacks;

public static class ModpackUtils
{
    public static ModModel[] LoadUnknownModsFromDirectory(InstanceModel instance)
    {
        string modsDirectory = Path.Combine(instance.Path, "mods");
        IEnumerable<string> foundMods = instance.Mods.Select(i => i.Name);
        string[] jars = Directory.GetFiles(modsDirectory, "*.jar", SearchOption.TopDirectoryOnly);

        List<ModModel> mods = new();
        foreach (string file in jars)
        {
            string name = Path.GetFileName(file);
            if (string.IsNullOrEmpty(name) || foundMods.Contains(name))
            {
                continue;
            }
            FabricModJson? fabricModJson = FabricLoader.GetLoaderFile(file);
            ModToml? modToml = ForgeLoader.GetLoaderFile(file);
            if (fabricModJson != null)
            {
                // Is Fabric mod.
            }
            else if (modToml != null)
            {
                // Is Forge mod.
            }
        }

        return mods.ToArray();
    }
}