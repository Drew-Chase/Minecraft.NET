/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

// Ignore Spelling: Modloader Modpack Utils modrinth curseforge

using Chase.Minecraft.Curseforge.Controller;
using Chase.Minecraft.Curseforge.Model;
using Chase.Minecraft.Data;
using Chase.Minecraft.Model;
using Chase.Minecraft.Modrinth;
using Chase.Minecraft.Modrinth.Controller;
using Chase.Minecraft.Modrinth.Model;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Collections.Concurrent;
using System.IO.Compression;

namespace Chase.Minecraft.Modpacks;

/// <summary>
/// Utility class for handling Minecraft modpack-related operations.
/// </summary>
public static class ModpackUtils
{
    public static string[] GetUnmappedMods(InstanceModel instance)
    {
        IEnumerable<string> jars = Directory.GetFiles(Path.Combine(instance.Path, "mods"), "*.jar", SearchOption.TopDirectoryOnly);
        IEnumerable<string> mappedMods = instance.Mods.Select(mod => mod.FileName);
        List<string> unmapped = new();
        foreach (string jarFile in jars)
        {
            if (!mappedMods.Contains(Path.GetFileName(jarFile)))
            {
                unmapped.Add(jarFile);
            }
        }

        return unmapped.ToArray();
    }

    public static int MapUnMappedMods(InstanceModel instance, ModrinthClient modrinthClient, CurseforgeClient? curseforgeClient = null)
    {
        int mapped = 0;
        ConcurrentQueue<ModModel> mods = new(instance.Mods);

        string[] unmapped = GetUnmappedMods(instance);

        Parallel.ForEach(unmapped, jarFile =>
        {
            if (TryMapVersionFromJar(jarFile, modrinthClient, out ModModel mod, curseforgeClient))
            {
                mods.Enqueue(mod);
                mapped++;
            }
        });

        instance.Mods = mods.ToArray();
        instance.InstanceManager.Save(instance.Id, instance);

        return mapped;
    }

    /// <summary>
    /// Tries to map the mods present in an instance's mods directory.
    /// </summary>
    /// <param name="instance">The instance model.</param>
    /// <param name="modrinthClient">The Modrinth client instance.</param>
    /// <param name="curseforgeClient">Optional Curseforge client instance.</param>
    /// <returns>An array of ModModel representing the mapped mods.</returns>
    public static ModModel[] TryMapInstanceMods(InstanceModel instance, ModrinthClient modrinthClient, CurseforgeClient? curseforgeClient = null) => TryMapDirectory(Path.Combine(instance.Path, "mods"), modrinthClient, curseforgeClient);

    /// <summary>
    /// Tries to map mods from a given directory.
    /// </summary>
    /// <param name="modsDirectory">The directory containing the mod JAR files.</param>
    /// <param name="modrinthClient">The Modrinth client instance.</param>
    /// <param name="curseforgeClient">Optional Curseforge client instance.</param>
    /// <returns>An array of ModModel representing the mapped mods.</returns>
    public static ModModel[] TryMapDirectory(string modsDirectory, ModrinthClient modrinthClient, CurseforgeClient? curseforgeClient = null)
    {
        string[] jars = Directory.GetFiles(modsDirectory, "*.jar", SearchOption.TopDirectoryOnly);

        ConcurrentQueue<ModModel> mods = new();
        Parallel.ForEach(jars, file =>
        {
            if (TryMapVersionFromJar(file, modrinthClient, out ModModel mod, curseforgeClient))
            {
                mods.Enqueue(mod);
            }
        });

        return mods.ToArray();
    }

    /// <summary>
    /// Attempts to map version information from a JAR file.
    /// </summary>
    /// <param name="file">The path to the JAR file.</param>
    /// <param name="modrinthClient">The Modrinth client instance.</param>
    /// <param name="mod">The mapped ModModel instance, if successful.</param>
    /// <param name="curseforgeClient">Optional Curseforge client instance.</param>
    /// <returns><c>true</c> if mapping was successful, otherwise <c>false</c>.</returns>
    public static bool TryMapVersionFromJar(string file, ModrinthClient modrinthClient, out ModModel mod, CurseforgeClient? curseforgeClient = null)
    {
        mod = new();
        try
        {
            PlatformSource source = PlatformSource.Unknown;
            string name = Path.GetFileName(file);
            FacetBuilder facets = new FacetBuilder().AddProjectTypes("mod");
            ModLoaders loader = ModLoaders.None;
            using (ZipArchive archive = ZipFile.OpenRead(file))
            {
                if (!AttemptToParseModNameFromJar(file, archive, out name))
                {
                    name = Path.GetFileName(file);
                }

                if (AttemptToParseModloaderFromJar(file, archive, out loader))
                {
                    facets.AddModloaders(loader);
                }
            }

            ModrinthSearchResult? searchResults = modrinthClient.Search(new()
            {
                Query = name,
                Limit = 3,
                Ordering = SearchOrdering.Relevance,
                Facets = facets
            });
            if (searchResults != null && searchResults.HasValue)
            {
                foreach (ModrinthSearchResultItem result in searchResults.Value.Hits)
                {
                    foreach (ModrinthVersionFile version in modrinthClient.GetProjectVersions(result.ProjectId) ?? Array.Empty<ModrinthVersionFile>())
                    {
                        foreach (VersionFileDetails versionFile in version.Files)
                        {
                            if (versionFile.Filename.Equals(Path.GetFileName(file), StringComparison.OrdinalIgnoreCase))
                            {
                                mod = new()
                                {
                                    ProjectID = result.ProjectId,
                                    VersionID = version.Id,
                                    Name = result.Title,
                                    Source = PlatformSource.Modrinth,
                                    DownloadURL = versionFile.Url,
                                    FileName = Path.GetFileName(file),
                                };
                                Log.Debug("Mapped mod file: {FILE} => {MOD}", file, mod.Name);
                                return true;
                            }
                        }
                    }
                }
            }

            if (curseforgeClient != null)
            {
                CurseforgeSearchResult? result = curseforgeClient.SearchModsAsync(name, "", loader).Result;
                if (result != null && result.HasValue)
                {
                    foreach (CurseforgeProject project in result.Value.Projects)
                    {
                        ModFile[] files = curseforgeClient.GetModFiles(project.Id.ToString()).Result ?? Array.Empty<ModFile>();
                        foreach (ModFile item in files)
                        {
                            if (item.FileName.Equals(Path.GetFileName(file), StringComparison.OrdinalIgnoreCase))
                            {
                                mod = new()
                                {
                                    ProjectID = project.Id.ToString(),
                                    Name = project.Name,
                                    VersionID = item.Id.ToString(),
                                    DownloadURL = item.DownloadUrl.ToString(),
                                    Source = PlatformSource.Curseforge,
                                    FileName = Path.GetFileName(file),
                                };
                                return true;
                            }
                        }
                    }
                }
            }
            name = Path.GetFileName(file);
            searchResults = modrinthClient.Search(new()
            {
                Query = name,
                Limit = 3,
                Ordering = SearchOrdering.Relevance,
                Facets = facets
            });
            if (searchResults != null && searchResults.HasValue)
            {
                foreach (ModrinthSearchResultItem result in searchResults.Value.Hits)
                {
                    foreach (ModrinthVersionFile version in modrinthClient.GetProjectVersions(result.ProjectId) ?? Array.Empty<ModrinthVersionFile>())
                    {
                        foreach (VersionFileDetails versionFile in version.Files)
                        {
                            if (versionFile.Filename.Equals(Path.GetFileName(file), StringComparison.OrdinalIgnoreCase))
                            {
                                mod = new()
                                {
                                    ProjectID = result.ProjectId,
                                    VersionID = version.Id,
                                    Name = result.Title,
                                    Source = PlatformSource.Modrinth,
                                    DownloadURL = versionFile.Url,
                                    FileName = Path.GetFileName(file),
                                };
                                Log.Debug("Mapped mod file: {FILE} => {MOD}", file, mod.Name);
                                return true;
                            }
                        }
                    }
                }
            }

            Log.Debug("Unable to Map mod file: {FILE}", file);
        }
        catch (Exception e)
        {
            Log.Error("Error occurred while mapping mod", e);
        }
        return false;
    }

    /// <summary>
    /// Attempts to parse modloader information from a JAR file.
    /// </summary>
    /// <param name="jar">The path to the JAR file.</param>
    /// <param name="loaders">The detected modloaders, if successful.</param>
    /// <param name="archive">
    /// the zip archive stream for the mods jar. See: <seealso cref="ZipFile.Open(string, ZipArchiveMode)"/>
    /// </param>
    /// <returns><c>true</c> if parsing was successful, otherwise <c>false</c>.</returns>
    public static bool AttemptToParseModloaderFromJar(string jar, ZipArchive archive, out ModLoaders loaders)
    {
        try
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.Equals("META-INF/mods.toml", StringComparison.OrdinalIgnoreCase))
                {
                    loaders = ModLoaders.Forge;
                    return true;
                }

                if (entry.FullName.Equals("fabric.mod.json", StringComparison.OrdinalIgnoreCase))
                {
                    loaders = ModLoaders.Fabric;
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error("Unable to parse mod loader from jar file: {FILE}", jar, e);
        }

        loaders = ModLoaders.None;
        return false;
    }

    /// <summary>
    /// Attempts to parse mod name information from a JAR file.
    /// </summary>
    /// <param name="jar">The path to the JAR file.</param>
    /// <param name="archive">
    /// the zip archive stream for the mods jar. See: <seealso cref="ZipFile.Open(string, ZipArchiveMode)"/>
    /// </param>
    /// <param name="name">The detected mod name, if successful.</param>
    /// <returns><c>true</c> if parsing was successful, otherwise <c>false</c>.</returns>
    public static bool AttemptToParseModNameFromJar(string jar, ZipArchive archive, out string name)
    {
        try
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                try
                {
                    if (entry.FullName.Equals("META-INF/mods.toml", StringComparison.OrdinalIgnoreCase))
                    {
                        using Stream stream = entry.Open();
                        using StreamReader reader = new(stream);
                        string? line = null;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Trim().StartsWith("displayName="))
                            {
                                name = line.Trim().Replace("displayName=\"", "").Trim('"');
                                return true;
                            }
                        }
                    }

                    if (entry.FullName.Equals("fabric.mod.json", StringComparison.OrdinalIgnoreCase))
                    {
                        using Stream stream = entry.Open();
                        using StreamReader reader = new(stream);
                        name = JObject.Parse(reader.ReadToEnd())["name"]?.ToObject<string>() ?? "";
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Unable to read archive entry: {FILE} - {ENTRY}", jar, entry.FullName, e);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error("Unable to parse mod name from jar {FILE}", jar, e);
        }

        name = "";
        return false;
    }
}