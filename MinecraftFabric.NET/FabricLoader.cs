/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Instances;
using Chase.Minecraft.Model;
using Chase.Networking;
using HtmlAgilityPack;

namespace Chase.Minecraft.Fabric;

public static class FabricLoader
{
    private static string maven = "https://maven.fabricmc.net/net/fabricmc/fabric-installer/";

    public static async Task Install(string version, InstanceManager manager, InstanceModel instance)
    {
        using NetworkClient client = new();

        string? latestInstallerVersion = await GetLatestInstallerVersion(client);
        if (latestInstallerVersion != null)
        {
            await client.DownloadFileAsync(new($"{maven}{latestInstallerVersion}"), "", (s, e) => { });
            instance.ModLoader = new()
            {
                Modloader = ModLoaders.Fabric,
                Version = version
            };
            manager.Save(instance.Id, instance);
        }
        else
        {
            throw new Exception($"Fabric installer version could not be found!");
        }
    }

    private static async Task<string?> GetLatestInstallerVersion(NetworkClient client)
    {
        using HttpResponseMessage response = await client.GetAsync(maven);
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