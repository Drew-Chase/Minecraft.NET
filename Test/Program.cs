﻿// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Modrinth;
using Chase.Minecraft.Modrinth.Model;
using System.Diagnostics;

namespace Test;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using ModrinthClient client = new();
        await SearchModrinth(client);
        await GetModrinthProject(client);
        await GetModrinthProjectDependencies(client);
        await GetModrinthUser(client);
        await GetProjectVersions(client);
        await DownloadVersionFile(client);
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task SearchModrinth(ModrinthClient client)
    {
        FacetBuilder builder = new FacetBuilder()
            .AddVersions("1.19.4")
            .AddProjectTypes(ModrinthProjectTypes.Mod)
            .AddModloaders(Chase.Minecraft.ModLoaders.Fabric);
        ModrinthSearchQuery query = new()
        {
            Facets = builder,
            Query = "The Warp Mod",
            Limit = 2,
            Ordering = SearchOrdering.Relevance,
        };

        ModrinthSearchResult? results = await client.SearchAsync(query);
        Console.ForegroundColor = results == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(results == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Modrinth Search!");
    }

    private static async Task GetModrinthProject(ModrinthClient client)
    {
        ModrinthProject? project = await client.GetProjectAsync("ewLFY6nv");
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Get Modrinth Project!");
    }

    private static async Task GetModrinthProjectDependencies(ModrinthClient client)
    {
        ModrinthProjectDependencies? dependencies = await client.GetProjectDependenciesAsync("ewLFY6nv");
        Console.ForegroundColor = dependencies == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(dependencies == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Get Modrinth Project Dependencies!");
    }

    private static async Task GetModrinthUser(ModrinthClient client)
    {
        ModrinthUser? user = await client.GetUserAsync("dcmanproductions");
        Console.ForegroundColor = user == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(user == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Get Modrinth User!");
    }

    private static async Task GetProjectVersions(ModrinthClient client)
    {
        var versions = await client.GetProjectVersionsAsync("ewLFY6nv");
        Console.ForegroundColor = versions == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(versions == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Get Project Versions!");
    }

    private static async Task DownloadVersionFile(ModrinthClient client)
    {
        var versions = await client.GetProjectVersionsAsync("ewLFY6nv");
        var version = versions?.First();
        bool success = false;
        if (version != null)
        {
            VersionFileDetails? file = version?.Files.FirstOrDefault(i => i.Primary);
            if (file != null)
            {
                await client.DownloadVersionFile(file.Value, Path.Combine(Directory.GetParent(Process.GetCurrentProcess().MainModule?.FileName ?? "")?.FullName ?? Environment.CurrentDirectory), (s, e) =>
                                {
                                    Console.WriteLine(e.Percentage.ToString("P2"));
                                });
                success = true;
            }
        }
        Console.ForegroundColor = !success ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(!success ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Get Project Versions!");
    }
}