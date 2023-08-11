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
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace Chase.Minecraft.Forge;

public static class ForgeLoader
{
    public static ModToml? GetLoaderFile(string jar)
    {
        using ZipArchive archive = ZipFile.OpenRead(jar);
        return GetLoaderFile(archive);
    }

    public static ModToml? GetLoaderFile(ZipArchive archive)
    {
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.FullName.Equals("META-INF/mods.toml", StringComparison.OrdinalIgnoreCase))
            {
                using Stream stream = entry.Open();
                using StreamReader reader = new(stream);
                string? line;
                string section = "";
                bool isDescription = false;
                Dependency? currentDependency = null;
                List<Dependency> dependencies = new();
                ModToml toml = new()
                {
                    Description = ""
                };
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith('#'))
                    {
                        continue;
                    }
                    int indexOfEquals = line.IndexOf('=');
                    int indexOfComment = line.IndexOf('#');
                    if (indexOfComment != -1)
                    {
                        line = line[..indexOfComment].Trim('#').Trim();
                    }
                    if (isDescription)
                    {
                        if (line.Equals("'''") || line.EndsWith("'''"))
                        {
                            isDescription = false;
                        }
                        else
                        {
                            toml.Description += "\n" + line;
                        }
                    }
                    if (line.StartsWith("[[mods]]"))
                    {
                        section = "mods";
                    }
                    else if (line.StartsWith($"[[dependencies.{toml.ModId}]]"))
                    {
                        section = "dependency";
                        if (currentDependency != null)
                        {
                            dependencies.Add(currentDependency);
                        }
                        currentDependency = new Dependency();
                    }
                    if (indexOfEquals != -1)
                    {
                        string value = line[indexOfEquals..].TrimStart('=').Trim().Trim('"').Trim();
                        if (line.StartsWith("issueTrackerURL"))
                        {
                            toml.IssueTrackerUrl = value;
                        }
                        else if (line.StartsWith("license"))
                        {
                            toml.License = value;
                        }

                        if (section == "mods")
                        {
                            if (line.StartsWith("modId"))
                            {
                                toml.ModId = value;
                            }
                            else if (line.StartsWith("version"))
                            {
                                toml.Version = value;
                            }
                            else if (line.StartsWith("displayName"))
                            {
                                toml.DisplayName = value;
                            }
                            else if (line.StartsWith("updateJSONURL"))
                            {
                                toml.UpdateJsonUrl = new Uri(value);
                            }
                            else if (line.StartsWith("displayURL"))
                            {
                                toml.DisplayUrl = value;
                            }
                            else if (line.StartsWith("logoFile"))
                            {
                                toml.LogoFile = value;
                            }
                            else if (line.StartsWith("authors"))
                            {
                                toml.Authors = value;
                            }
                            else if (line.StartsWith("description"))
                            {
                                if (value.Equals("'''"))
                                {
                                    isDescription = true;
                                }
                                else if (value.StartsWith("'''"))
                                {
                                    isDescription = true;
                                    toml.Description += value.Replace("'''", "").Trim();
                                }
                                else
                                {
                                    toml.Description = value;
                                }
                            }
                            else if (line.Equals("'''") || line.EndsWith("'''"))
                            {
                                isDescription = false;
                            }
                        }
                        else if (section.Equals("dependency"))
                        {
                            currentDependency ??= new();
                            if (line.StartsWith("modId"))
                            {
                                currentDependency.ModId = value;
                            }
                            else if (line.StartsWith("mandatory"))
                            {
                                currentDependency.Mandatory = bool.Parse(value);
                            }
                            else if (line.StartsWith("versionRange"))
                            {
                                currentDependency.VersionRange = value;
                            }
                            else if (line.StartsWith("ordering"))
                            {
                                currentDependency.Ordering = (ModOrdering)Enum.Parse(typeof(ModOrdering), value);
                            }
                            else if (line.StartsWith("side"))
                            {
                                currentDependency.Side = (Side)Enum.Parse(typeof(Side), value);
                            }
                        }
                    }
                }
                if (currentDependency != null)
                {
                    dependencies.Add(currentDependency);
                }
                toml.Dependencies = dependencies.ToArray();
                toml.Description = toml.Description.Trim('\n').Trim();
                return toml;
            }
        }
        return null;
    }

    public static async Task<InstanceModel?> Install(string loaderVersion, InstanceModel instance, string rootDirectory, string javaExecutable)
    {
        using NetworkClient client = new();
        string installerPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
        Uri url = new($"https://maven.minecraftforge.net/net/minecraftforge/forge/{instance.MinecraftVersion.ID}-{loaderVersion}/forge-{instance.MinecraftVersion.ID}-{loaderVersion}-installer.jar");
        await client.DownloadFileAsync(url, installerPath, (_, _) => { });
        string wrapper = ExtractWrapper();
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
            string librariesPath = Path.Combine(rootDirectory, "libraries");
            instance.JVMArguments = info.Arguments.Jvm.Select(i => i.Replace("${library_directory}", librariesPath).Replace("${classpath_separator}", ";").Replace("${version_name}", info.Id)).ToArray();
            instance.ModLoader = new()
            {
                Modloader = ModLoaders.Forge,
                Version = loaderVersion
            };

            string fmlVersion = "";

            for (int i = 0; i < instance.JVMArguments.Length; i++)
            {
                string name = instance.JVMArguments[i];
                if (name == "-p")
                {
                    instance.JVMArguments[i + 1] = $"\"{instance.JVMArguments[i + 1]}\"";
                }
                if (name.StartsWith("-DlibraryDirectory="))
                {
                    instance.JVMArguments[i] = $"-DlibraryDirectory=\"{librariesPath}\"";
                }
            }

            for (int i = 0; i < info.Arguments.Game.Length; i++)
            {
                string name = info.Arguments.Game[i];

                if (name == "--fml.mcpVersion")
                {
                    fmlVersion = info.Arguments.Game[i + 1];
                }
            }

            List<Task> tasks = new();
            List<string> paths = new();
            foreach (ForgeLibrary libraryItem in info.Libraries)
            {
                ForgeArtifact artifact = libraryItem.Downloads.Artifact;
                Uri artifactUri = artifact.Url;
                string artifactPath = Path.Combine(librariesPath, artifact.Path);
                Directory.CreateDirectory(Directory.GetParent(artifactPath)?.FullName ?? "");
                paths.Add(artifactPath);
            }
            Task.WaitAll(tasks.ToArray());
            instance.LaunchClassPath = info.MainClass;
            instance.AdditionalClassPaths = paths.ToArray();
            instance.ClientJar = Path.Combine(rootDirectory, "versions", info.Id, $"{info.Id}.jar");
            instance.GameVersion = info.Id;
            instance = instance.InstanceManager.Save(instance.Id, instance);

            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = javaExecutable,
                    Arguments = $"-jar \"{wrapper}\" -i \"{installerPath}\" -o \"{rootDirectory}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                },
                EnableRaisingEvents = true,
            };
            process.OutputDataReceived += (s, e) =>
            {
                string? data = e.Data;
                if (data != null)
                {
                    Log.Debug("[FORGE] " + data);
                }
            };
            process.ErrorDataReceived += (s, e) =>
            {
                string? data = e.Data;
                if (data != null)
                {
                    Log.Error("[FORGE] " + data);
                }
            };

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();

            return instance;
        }
        return null;
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

    public static async Task<string> InstallServer(string directory, string loaderVersion, MinecraftVersion minecraftVersion, string javaExe, DataReceivedEventHandler? installDataHandler = null)
    {
        directory = Directory.CreateDirectory(directory).FullName;
        string installer = Path.Combine(directory, "forge-installer.jar");
        using NetworkClient client = new();
        Uri url = new($"https://maven.minecraftforge.net/net/minecraftforge/forge/{minecraftVersion.ID}-{loaderVersion}/forge-{minecraftVersion.ID}-{loaderVersion}-installer.jar");
        await client.DownloadFileAsync(url, installer);
        Process process = new()
        {
            StartInfo = new()
            {
                FileName = javaExe,
                Arguments = $"-jar {installer} -installServer .",
                WorkingDirectory = directory,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            },
            EnableRaisingEvents = true,
        };

        if (installDataHandler != null)
        {
            process.OutputDataReceived += installDataHandler;
        }
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        File.Delete(installer);

        return installer;
    }

    private static string ExtractWrapper()
    {
        string path = Path.Combine(Path.GetTempPath(), "forge-wrapper.jar");

        if (!File.Exists(path))
        {
            using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Chase.Minecraft.Forge.jars.ForgeWrapper.jar");
            if (stream != null)
            {
                using FileStream fs = new(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                stream.CopyTo(fs);
            }
        }

        return path;
    }
}