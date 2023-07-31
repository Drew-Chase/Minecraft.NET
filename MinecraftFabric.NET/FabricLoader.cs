/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Data;
using Chase.Minecraft.Model;
using Chase.Networking;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;

namespace Chase.Minecraft.Fabric;

/// <summary>
/// A static class that handles the installation and management of Fabric modloader for Minecraft instances.
/// </summary>
public static class FabricLoader
{
    private static readonly string MavenURL = "https://maven.fabricmc.net";
    private static readonly string MavenInstallerPath = "/net/fabricmc/fabric-installer/";

    public static FabricModJson? GetLoaderFile(string jar)
    {
        using ZipArchive archive = ZipFile.OpenRead(jar);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.FullName.Equals("fabric.mod.json", StringComparison.OrdinalIgnoreCase))
            {
                using Stream stream = entry.Open();
                using StreamReader reader = new(stream);
                return JObject.Parse(reader.ReadToEnd()).ToObject<FabricModJson>();
            }
        }
        return null;
    }

    /// <summary>
    /// Install the specified version of Fabric modloader for the given Minecraft instance.
    /// </summary>
    /// <param name="version">The version of Fabric modloader to install.</param>
    /// <param name="instance">The Minecraft instance model for which to install the modloader.</param>
    /// <returns>A task representing the asynchronous installation process.</returns>
    /// <exception cref="Exception">Thrown when the Fabric installer version could not be found.</exception>
    public static async Task Install(string version, InstanceModel instance)
    {
        using NetworkClient client = new();

        string librariesDirectory = Path.Combine(instance.Path, "libraries");

        if (Directory.Exists(librariesDirectory))
        {
            Directory.Delete(librariesDirectory, true);
        }

        string? latestInstallerVersion = await GetLatestInstallerVersion(client);
        if (latestInstallerVersion != null)
        {
            string temp = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            Uri url = new($"{MavenURL}{MavenInstallerPath}{latestInstallerVersion}/fabric-installer-{latestInstallerVersion}.jar");
            await client.DownloadFileAsync(url, temp, (s, e) => { });
            instance.ModLoader = new()
            {
                Modloader = ModLoaders.Fabric,
                Version = version,
            };
            await Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = instance.Java,
                    Arguments = $"-jar {temp} client -dir \"{instance.Path}\" -loader {version} -noprofile",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                })?.WaitForExit();
            });
            instance.LaunchClassPath = "\"-DFabricMcEmu= net.minecraft.client.main.Main\" net.fabricmc.loader.impl.launch.knot.KnotClient";
            List<string> additionalPaths = instance.AdditionalClassPaths.ToList();
            foreach (string file in Directory.GetFiles(Directory.CreateDirectory(librariesDirectory).FullName, "*.jar", SearchOption.AllDirectories))
            {
                if (!additionalPaths.Contains(file))
                {
                    additionalPaths.Add(file);
                }
            }
            instance.AdditionalClassPaths = additionalPaths.ToArray();
            instance.InstanceManager.Save(instance.Id, instance);
        }
        else
        {
            throw new Exception($"Fabric installer version could not be found!");
        }
    }

    /// <summary>
    /// Get a list of available Fabric modloader versions.
    /// </summary>
    /// <returns>An array of strings representing the available Fabric modloader versions.</returns>
    public static async Task<string[]> GetLoaderVersions()
    {
        using NetworkClient client = new();
        using HttpResponseMessage response = await client.GetAsync(MavenURL + "/net/fabricmc/fabric-loader/");
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            HtmlDocument document = new();
            document.LoadHtml(content);
            if (document.DocumentNode != null)
            {
                List<string> versions = new();
                HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("/html/body/pre/a");
                foreach (HtmlNode node in nodes)
                {
                    string innerText = node.InnerText.Trim('/');
                    if (int.TryParse(innerText[0].ToString(), out _))
                    {
                        versions.Add(innerText);
                    }
                }
                versions.Sort(new VersionStringComparer());
                versions.Reverse();
                return versions.ToArray();
            }
        }
        return Array.Empty<string>();
    }

    /// <summary>
    /// Get the latest version of the Fabric installer.
    /// </summary>
    /// <param name="client">The NetworkClient used for making HTTP requests.</param>
    /// <returns>The latest version of the Fabric installer as a string, or null if not found.</returns>
    private static async Task<string?> GetLatestInstallerVersion(NetworkClient client)
    {
        using HttpResponseMessage response = await client.GetAsync(MavenURL + MavenInstallerPath);
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            HtmlDocument document = new();
            document.LoadHtml(content);
            if (document.DocumentNode != null)
            {
                List<Version> versions = new();
                HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("/html/body/pre/a");
                foreach (HtmlNode node in nodes)
                {
                    string innerText = node.InnerText.Trim('/'); ;
                    if (Version.TryParse(innerText, out Version? version) && version != null)
                    {
                        versions.Add(version);
                    }
                }
                return versions.OrderByDescending(v => v).First().ToString();
            }
        }

        return null;
    }
}