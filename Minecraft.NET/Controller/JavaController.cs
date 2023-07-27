﻿/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Exceptions;
using Chase.Networking;
using Chase.Networking.Event;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Runtime.InteropServices;

namespace Chase.Minecraft.Controller;

public struct JVMInstallations
{
    public string? Latest { get; set; }
    public string? Legacy { get; set; }
}

public static class JavaController
{
    public static JVMInstallations GetLocalJVMInstallations(string path)
    {
        string latest = Path.Combine(path, "java-latest", "bin", "java.exe");
        string legacy = Path.Combine(path, "java-legacy", "bin", "java.exe");
        JVMInstallations installations = new()
        {
            Latest = null,
            Legacy = null,
        };
        if (File.Exists(latest))
        {
            installations.Latest = latest;
        }
        if (File.Exists(legacy))
        {
            installations.Legacy = legacy;
        }

        return installations;
    }

    public static string[] GetGlobalJVMInstallations() => Environment.GetEnvironmentVariable("PATH")?.Split(";")?.Where(i => i.Contains("jdk") || i.Contains("jre") || i.Contains("java")).ToArray() ?? Array.Empty<string>();

    public static async Task DownloadJava(string path, DownloadProgressEvent? javaLatestProgressEvent = null, DownloadProgressEvent? javaLegacyProgressEvent = null, bool force = false)
    {
        path = Directory.CreateDirectory(path).FullName;
        using NetworkClient client = new();
        string? java8 = null, javaLatest = null;
        string? key = null;
        if (OperatingSystem.IsWindows())
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
            {
                key = "windows-arm64";
            }
            else
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    key = "windows-x64";
                }
                else
                {
                    key = "windows-x86";
                }
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm)
            {
                key = "linux";
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm)
            {
                key = "mac-os";
            }
        }
        else if (OperatingSystem.IsMacCatalyst())
        {
            key = "mac-os-arm64";
        }

        if (key == null)
        {
            throw new InvalidSystemException(new()
                    {
                        { OSPlatform.Linux, System.Reflection.ProcessorArchitecture.Amd64 },
                        { OSPlatform.OSX, System.Reflection.ProcessorArchitecture.Amd64 },
                        { OSPlatform.OSX, System.Reflection.ProcessorArchitecture.Arm},
                        { OSPlatform.Windows, System.Reflection.ProcessorArchitecture.Amd64},
                        { OSPlatform.Windows, System.Reflection.ProcessorArchitecture.X86},
                        { OSPlatform.Windows, System.Reflection.ProcessorArchitecture.Arm},
                    });
        }
        JObject? json = null;
        using (HttpResponseMessage response = await client.GetAsync("https://piston-meta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json"))
        {
            if (response.IsSuccessStatusCode)
            {
                json = JObject.Parse(await response.Content.ReadAsStringAsync())[key]?.ToObject<JObject>();
            }
        }
        if (json != null)
        {
            javaLatest = json["java-runtime-gamma"]?[0]?["manifest"]?["url"]?.ToObject<string>();
            java8 = json["jre-legacy"]?[0]?["manifest"]?["url"]?.ToObject<string>();
        }
        List<Task> tasks = new();
        if (javaLatest != null)
        {
            string installation = Path.Combine(path, "java-latest");
            if (!File.Exists(Path.Combine(installation, "bin", "java.exe")) || force)
            {
                javaLatestProgressEvent ??= (s, e) => { };
                tasks.Add(DownloadJavaManifest(installation, javaLatest, client, javaLatestProgressEvent));
            }
        }
        if (java8 != null)
        {
            string installation = Path.Combine(path, "java-legacy");
            if (!File.Exists(Path.Combine(installation, "bin", "java.exe")) || force)
            {
                javaLegacyProgressEvent ??= (s, e) => { };
                tasks.Add(DownloadJavaManifest(installation, java8, client, javaLegacyProgressEvent));
            }
        }
        Task.WaitAll(tasks.ToArray());
    }

    private static async Task DownloadJavaManifest(string path, string manifest, NetworkClient client, DownloadProgressEvent progressEvent)
    {
        path = Directory.CreateDirectory(path).FullName;
        JObject? files = null;
        using (HttpResponseMessage response = await client.GetAsync(manifest))
        {
            if (response.IsSuccessStatusCode)
            {
                files = JObject.Parse(await response.Content.ReadAsStringAsync())["files"]?.ToObject<JObject>();
            }
        }
        if (files != null)
        {
            List<Task> tasks = new();
            foreach (JProperty item in files.Properties())
            {
                string filepath = Path.Combine(path, item.Name);
                Directory.CreateDirectory(Directory.GetParent(filepath)?.FullName ?? "");
                string url = item.Value["downloads"]?["raw"]?["url"]?.ToObject<string>() ?? "";
                if (!string.IsNullOrWhiteSpace(url))
                {
                    try
                    {
                        Log.Debug($"Downloading {item.Name}");
                        tasks.Add(client.DownloadFileAsync(url, filepath, progressEvent));
                    }
                    catch { }
                }
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}