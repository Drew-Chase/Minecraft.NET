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
using System.Diagnostics;

namespace Chase.Minecraft.Fabric;

public static class FabricLoader
{
    private static readonly string MavenURL = "https://maven.fabricmc.net";
    private static readonly string MavenInstallerPath = "/net/fabricmc/fabric-installer/";

    public static async Task Install(string version, InstanceModel instance)
    {
        using NetworkClient client = new();

        string? latestInstallerVersion = await GetLatestInstallerVersion(client);
        if (latestInstallerVersion != null)
        {
            string temp = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            await client.DownloadFileAsync(new($"{MavenURL}{MavenInstallerPath}{latestInstallerVersion}/fabric-installer-{latestInstallerVersion}.jar"), temp, (s, e) => { });
            instance.ModLoader = new()
            {
                Modloader = ModLoaders.Fabric,
                Version = version
            };
            instance.LaunchClassPath = "\"-DFabricMcEmu= net.minecraft.client.main.Main\" net.fabricmc.loader.impl.launch.knot.KnotClient";
            List<string> additionalPaths = instance.AdditionalClassPaths.ToList();
            foreach (string file in Directory.GetFiles(Path.Combine(instance.Path, "libraries"), "*.jar", SearchOption.AllDirectories))
            {
                if (!additionalPaths.Contains(file))
                {
                    additionalPaths.Add(file);
                }
            }
            instance.AdditionalClassPaths = additionalPaths.ToArray();
            instance.InstanceManager.Save(instance.Id, instance);
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
        }
        else
        {
            throw new Exception($"Fabric installer version could not be found!");
        }
    }

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