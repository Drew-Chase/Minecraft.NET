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

namespace Chase.Minecraft.Forge;

public static class ForgeLoader
{
    public static async Task Install(string version, InstanceModel instance)
    {
        using NetworkClient client = new();
    }

    public static async Task<string[]> GetLoaderVersions(string version)
    {
        using NetworkClient client = new();
        using HttpResponseMessage response = await client.GetAsync($"https://files.minecraftforge.net/net/minecraftforge/forge/index_{version}.html");
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