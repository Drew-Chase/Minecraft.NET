// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Modrinth;
using Chase.Minecraft.Modrinth.Model;

namespace Test;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await SearchModrinth();
        await GetModrinthProject();
        await GetModrinthProjectDependencies();
        await GetModrinthUser();
        await GetProjectVersions();
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task SearchModrinth()
    {
        using ModrinthClient client = new();
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

    private static async Task GetModrinthProject()
    {
        using ModrinthClient client = new();
        ModrinthProject? project = await client.GetProjectAsync("ewLFY6nv");
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Get Modrinth Project!");
    }

    private static async Task GetModrinthProjectDependencies()
    {
        using ModrinthClient client = new();
        ModrinthProjectDependencies? dependencies = await client.GetProjectDependenciesAsync("ewLFY6nv");
        Console.ForegroundColor = dependencies == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(dependencies == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Get Modrinth Project Dependencies!");
    }

    private static async Task GetModrinthUser()
    {
        using ModrinthClient client = new();
        ModrinthUser? user = await client.GetUserAsync("dcmanproductions");
        Console.ForegroundColor = user == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(user == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Get Modrinth User!");
    }

    private static async Task GetProjectVersions()
    {
        using ModrinthClient client = new();
        var versions = await client.GetProjectVersionsAsync("ewLFY6nv");
        Console.ForegroundColor = versions == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(versions == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Console.WriteLine($" Get Project Versions!");
    }
}