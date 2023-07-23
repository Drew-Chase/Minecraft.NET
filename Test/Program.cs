// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Modrinth;
using Chase.Minecraft.Modrinth.Model;
using Newtonsoft.Json;

namespace Test;

internal class Program
{
    private static void Main(string[] args)
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

        ModrinthSearchResult? results = client.Search(query);
        Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }
}