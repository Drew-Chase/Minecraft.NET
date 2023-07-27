/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Instances;
using Chase.Minecraft.Model;
using Chase.Minecraft.Modrinth;
using Chase.Minecraft.Modrinth.Controller;
using Chase.Minecraft.Modrinth.Data;
using Chase.Minecraft.Modrinth.Model;
using Serilog;

namespace Test;

internal class ModrinthTest
{
    public static async Task Start()
    {
        using ModrinthClient client = new();
        await SearchModrinth(client);
        await GetModrinthProject(client);
        await GetModrinthProjectDependencies(client);
        await GetModrinthUser(client);
        await GetProjectVersions(client);
        await GetCategories(client);
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
        Log.Debug($" Modrinth Search!");
    }

    private static async Task GetModrinthProject(ModrinthClient client)
    {
        ModrinthProject? project = await client.GetProjectAsync("ewLFY6nv");
        Console.ForegroundColor = project == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(project == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Log.Debug($" Get Modrinth Project!");
    }

    private static async Task GetModrinthProjectDependencies(ModrinthClient client)
    {
        ModrinthProjectDependencies? dependencies = await client.GetProjectDependenciesAsync("ewLFY6nv");
        Console.ForegroundColor = dependencies == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(dependencies == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Log.Debug($" Get Modrinth Project Dependencies!");
    }

    private static async Task GetModrinthUser(ModrinthClient client)
    {
        ModrinthUser? user = await client.GetUserAsync("dcmanproductions");
        Console.ForegroundColor = user == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(user == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Log.Debug($" Get Modrinth User!");
    }

    private static async Task GetProjectVersions(ModrinthClient client)
    {
        var versions = await client.GetProjectVersionsAsync("ewLFY6nv");
        Console.ForegroundColor = versions == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(versions == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Log.Debug($" Get Project Versions!");
    }

    private static async Task GetCategories(ModrinthClient client)
    {
        var categories = await client.GetCategoriesAsync();
        Console.ForegroundColor = categories == null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(categories == null ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Log.Debug($" Get Categories!");
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
                InstanceManager manager = new(Path.GetFullPath("./minecraft/instances"));
                await client.DownloadVersionFile(file.Value, manager.GetFirstInstancesByName("Test"));
                success = true;
            }
        }
        Console.ForegroundColor = !success ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{(!success ? "FAIL" : "SUCCESS")}]");
        Console.ResetColor();
        Log.Debug($" Get Project Version File!");
    }

    private static async Task Overview()
    {
        InstanceModel instance = new();

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
        using ModrinthClient client = new(); // This creates a Modrinth client that can be used to make various api calls.
        CategoryTag[]? categories = await client.GetCategoriesAsync(); // Gets a list of all Modrinth Categories.
        ModrinthSearchResult? results = await client.SearchAsync(query); // This searches the modrinth api with the specified query
        ModrinthSearchResultItem topResult = results.Value.Hits[0]; // Gets the top result
        ModrinthVersionFile[]? projectVersions = await client.GetProjectVersionsAsync(topResult.ProjectId); // Gets a list of all of the projects versions
        string filePath = await client.DownloadVersionFile(projectVersions[0].Files[0], instance, "mods"); // Downloads the latest mod version and place it in the "mods" directory inside the instance directory.
        ModrinthProjectDependencies? dependencies = await client.GetProjectDependenciesAsync(topResult.ProjectId); // Gets a list of all of the projects dependencies.
        ModrinthUser? user = await client.GetUserAsync("dcmanproductions"); // Gets a Modrinth User based on their username.
        ModrinthProject[] projects = user?.Projects ?? Array.Empty<ModrinthProject>(); // Gets a list of the users projects.
    }
}