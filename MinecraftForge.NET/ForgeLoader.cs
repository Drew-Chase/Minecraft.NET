/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Data;
using Chase.Minecraft.Forge.Model;
using Chase.Minecraft.Model;
using Chase.Networking;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Serilog;
using System.IO.Compression;

namespace Chase.Minecraft.Forge;

public static class ForgeLoader
{
    public static async Task Install(string loader_version, InstanceModel instance)
    {
        using NetworkClient client = new();
        string installerPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
        Uri url = new($"https://maven.minecraftforge.net/net/minecraftforge/forge/{instance.MinecraftVersion.ID}-{loader_version}/forge-{instance.MinecraftVersion.ID}-{loader_version}-installer.jar");
        await client.DownloadFileAsync(url, installerPath, (_, _) => { });

        string jsonFileName = "version.json";
        ForgeVersionInfo info = new();
        using (ZipArchive archive = ZipFile.OpenRead(installerPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.Equals(jsonFileName, StringComparison.OrdinalIgnoreCase))
                {
                    using Stream stream = entry.Open();
                    using StreamReader reader = new(stream);
                    string jsonContent = reader.ReadToEnd();
                    info = JsonConvert.DeserializeObject<ForgeVersionInfo>(jsonContent);
                }
            }
        }
        if (!string.IsNullOrWhiteSpace(info.Id))
        {
            instance.MinecraftArguments = info.Arguments.Game;
            instance.JVMArguments = info.Arguments.Jvm.Select(i => i.Replace("${library_directory}", Path.Combine(instance.Path, "libraries")).Replace("${classpath_separator}", ";").Replace("${version_name}", info.Id)).ToArray();
            instance.ModLoader = new()
            {
                Modloader = ModLoaders.Forge,
                Version = loader_version
            };

            List<Task> tasks = new();
            foreach (ForgeLibrary libraryItem in info.Libraries)
            {
                ForgeArtifact artifact = libraryItem.Downloads.Artifact;
                Uri artifactUri = artifact.Url;
                string artifactPath = Path.Combine(Path.Combine(instance.Path, "libraries"), artifact.Path);
                Directory.CreateDirectory(Directory.GetParent(artifactPath)?.FullName ?? "");
                Log.Debug($"[Forge] Downloading {artifact.Path}");
                tasks.Add(client.DownloadFileAsync(artifactUri, artifactPath, (_, _) => { }));
            }
            Task.WaitAll(tasks.ToArray());

            instance.InstanceManager.Save(instance.Id, instance);
        }
    }

    public static async Task<string[]> GetLoaderVersions(string minecraft_version)
    {
        using NetworkClient client = new();
        using HttpResponseMessage response = await client.GetAsync($"https://files.minecraftforge.net/net/minecraftforge/forge/index_{minecraft_version}.html");
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            HtmlDocument document = new();
            document.LoadHtml(content);
            if (document.DocumentNode != null)
            {
                List<string> versions = new();
                HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//td[@class='download-version']");
                foreach (HtmlNode node in nodes)
                {
                    string innerText = node.InnerText.Trim('/').Trim();
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
}