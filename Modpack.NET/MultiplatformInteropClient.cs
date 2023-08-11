/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Curseforge.Controller;
using Chase.Minecraft.Modpacks.Model;
using Chase.Minecraft.Modrinth;
using Chase.Minecraft.Modrinth.Controller;
using SearchNET.Algorithm;
using System.Diagnostics;

namespace Chase.Minecraft.Modpacks;

/// <summary>
/// Provides methods to search for Minecraft mods, modpacks, resourcepacks, worlds, shaderpacks, and datapacks.
/// </summary>
public static class MultiPlatformInteropClient
{
    /// <summary>
    /// Searches for mods based on the provided search criteria.
    /// </summary>
    /// <param name="searchBuilder">The search criteria builder.</param>
    /// <returns>A collection of search results containing mods.</returns>
    public static async Task<SearchResults<Mod>> SearchModsAsync(SearchBuilder searchBuilder)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<Mod> mods = new();
        int totalResults = 0;

        if (!searchBuilder.platforms.Any() || searchBuilder.platforms.Contains(Minecraft.Data.PlatformSource.Modrinth))
        {
            using ModrinthClient client = new();
            FacetBuilder builder = new();

            if (!searchBuilder.minecraftVersions.Any())
            {
                builder.AddVersions(searchBuilder.minecraftVersions.Select(i => i.ID).ToArray());
            }

            if (!searchBuilder.loaders.Any())
            {
                builder.AddModloaders(searchBuilder.loaders.ToArray());
            }

            builder.AddProjectTypes(Modrinth.Data.ModrinthProjectTypes.Mod);

            Modrinth.Model.ModrinthSearchResult? result = await client.SearchAsync(new Modrinth.Model.ModrinthSearchQuery()
            {
                Query = searchBuilder.query,
                Facets = builder,
                Limit = 100,
                Offset = searchBuilder.offset,
                Ordering = Modrinth.Data.SearchOrdering.Relevance,
            });

            if (result != null)
            {
                totalResults += result.Value.TotalHits;
                Modrinth.Model.ModrinthSearchResultItem[] hits = result.Value.Hits;
                foreach (var hit in hits)
                {
                    mods.Add(new()
                    {
                        ID = hit.ProjectId,
                        Title = hit.Title,
                        Description = hit.Description,
                        Author = hit.Author,
                        Icon = hit.IconUrl,
                        Banner = hit.FeaturedGallery,
                        Categories = hit.Categories,
                        ClientSide = hit.ClientSide,
                        ServerSide = hit.ServerSide,
                        Downloads = hit.Downloads,
                        Platform = Minecraft.Data.PlatformSource.Modrinth,
                        GameVersions = hit.Versions,
                        Versions = Array.Empty<ResourceItemVersion>(),
                        SupportedLoaders = Array.Empty<ModLoaders>(),
                        ReleaseDate = hit.DateCreated,
                        LastUpdated = hit.DateModified,
                        IsDistributionAllowed = true,
                    });
                }
            }
        }
        if (!searchBuilder.platforms.Any() || searchBuilder.platforms.Contains(Minecraft.Data.PlatformSource.Curseforge))
        {
            if (searchBuilder.apiPlatform.ContainsKey(Minecraft.Data.PlatformSource.Curseforge))
            {
                string apiKey = searchBuilder.apiPlatform[Minecraft.Data.PlatformSource.Curseforge];
                using CurseforgeClient client = new(apiKey);
                Curseforge.Model.CurseforgeSearchResult? result = await client.SearchModsAsync(searchBuilder.query, searchBuilder.minecraftVersions.First().ID, searchBuilder.loaders.First());
                if (result != null)
                {
                    totalResults += result.Value.Pagination.TotalCount;
                    foreach (var item in result.Value.Projects)
                    {
                        mods.Add(new()
                        {
                            ID = item.Id.ToString(),
                            Title = item.Name,
                            Author = item.Authors.FirstOrDefault().Name,
                            ClientSide = Modrinth.Data.SideRequirements.Required,
                            ServerSide = Modrinth.Data.SideRequirements.Required,
                            Description = item.Summary,
                            Icon = item.Logo.ThumbnailUrl,
                            Downloads = item.DownloadCount,
                            LastUpdated = item.DateModified,
                            ReleaseDate = item.DateCreated,
                            Platform = Minecraft.Data.PlatformSource.Curseforge,
                            IsDistributionAllowed = item.AllowModDistribution,
                            Versions = Array.Empty<ResourceItemVersion>(),
                            SupportedLoaders = Array.Empty<ModLoaders>(),
                            Banner = "",
                            GameVersions = new string[] { searchBuilder.minecraftVersions.First().ID },
                            Categories = Array.Empty<string>(),
                        });
                    }
                }
            }
        }

        stopwatch.Stop();
        mods = mods.Distinct().ToList();
        FuzzySearchAlgorithm search = new();
        search.Add(mods.Select(i => i.Title).ToArray());
        string[]? items = search.Search(searchBuilder.query, searchBuilder.limit);
        if (items != null)
        {
            mods = mods.OrderByDescending(mod => items.Select((title, index) => (title, index)).FirstOrDefault(tuple => tuple.title == mod.Title).index).ToList();
        }

        return new SearchResults<Mod>()
        {
            Duration = stopwatch.Elapsed,
            Limit = searchBuilder.limit,
            Offset = searchBuilder.offset,
            Results = mods.Take(searchBuilder.limit).ToArray(),
            TotalResults = totalResults,
        };
    }

    /// <summary>
    /// Searches for modpacks based on the provided search criteria.
    /// </summary>
    /// <param name="searchBuilder">The search criteria builder.</param>
    /// <returns>A collection of search results containing modpacks.</returns>
    public static async Task<SearchResults<Modpack>> SearchModpacksAsync(SearchBuilder searchBuilder)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<Modpack> packs = new();
        int totalResults = 0;

        if (!searchBuilder.platforms.Any() || searchBuilder.platforms.Contains(Minecraft.Data.PlatformSource.Modrinth))
        {
            using ModrinthClient client = new();
            FacetBuilder builder = new();

            if (!searchBuilder.minecraftVersions.Any())
            {
                builder.AddVersions(searchBuilder.minecraftVersions.Select(i => i.ID).ToArray());
            }

            if (!searchBuilder.loaders.Any())
            {
                builder.AddModloaders(searchBuilder.loaders.ToArray());
            }

            builder.AddProjectTypes(Modrinth.Data.ModrinthProjectTypes.Modpack);

            Modrinth.Model.ModrinthSearchResult? result = await client.SearchAsync(new Modrinth.Model.ModrinthSearchQuery()
            {
                Query = searchBuilder.query,
                Facets = builder,
                Limit = 100,
                Offset = searchBuilder.offset,
                Ordering = Modrinth.Data.SearchOrdering.Relevance,
            });

            if (result != null)
            {
                totalResults += result.Value.TotalHits;
                Modrinth.Model.ModrinthSearchResultItem[] hits = result.Value.Hits;
                foreach (var hit in hits)
                {
                    packs.Add(new()
                    {
                        ID = hit.ProjectId,
                        Title = hit.Title,
                        Description = hit.Description,
                        Author = hit.Author,
                        Icon = hit.IconUrl,
                        Banner = hit.FeaturedGallery,
                        Categories = hit.Categories,
                        Downloads = hit.Downloads,
                        Platform = Minecraft.Data.PlatformSource.Modrinth,
                        GameVersions = hit.Versions,
                        Versions = Array.Empty<ResourceItemVersion>(),
                        SupportedLoaders = Array.Empty<ModLoaders>(),
                        ReleaseDate = hit.DateCreated,
                        LastUpdated = hit.DateModified,
                        IsDistributionAllowed = true,
                    });
                }
            }
        }
        if (!searchBuilder.platforms.Any() || searchBuilder.platforms.Contains(Minecraft.Data.PlatformSource.Curseforge))
        {
            if (searchBuilder.apiPlatform.ContainsKey(Minecraft.Data.PlatformSource.Curseforge))
            {
                string apiKey = searchBuilder.apiPlatform[Minecraft.Data.PlatformSource.Curseforge];
                using CurseforgeClient client = new(apiKey);
                Curseforge.Model.CurseforgeSearchResult? result = await client.SearchModpackAsync(searchBuilder.query, searchBuilder.minecraftVersions.First().ID, searchBuilder.loaders.First());
                if (result != null)
                {
                    totalResults += result.Value.Pagination.TotalCount;
                    foreach (var item in result.Value.Projects)
                    {
                        packs.Add(new()
                        {
                            ID = item.Id.ToString(),
                            Title = item.Name,
                            Author = item.Authors.FirstOrDefault().Name,
                            Description = item.Summary,
                            Icon = item.Logo.ThumbnailUrl,
                            Downloads = item.DownloadCount,
                            LastUpdated = item.DateModified,
                            ReleaseDate = item.DateCreated,
                            Platform = Minecraft.Data.PlatformSource.Curseforge,
                            IsDistributionAllowed = item.AllowModDistribution,
                            Versions = Array.Empty<ResourceItemVersion>(),
                            SupportedLoaders = Array.Empty<ModLoaders>(),
                            Banner = "",
                            GameVersions = new string[] { searchBuilder.minecraftVersions.First().ID },
                            Categories = Array.Empty<string>(),
                        });
                    }
                }
            }
        }

        stopwatch.Stop();
        packs = packs.Distinct().ToList();
        FuzzySearchAlgorithm search = new();
        search.Add(packs.Select(i => i.Title).ToArray());
        string[]? items = search.Search(searchBuilder.query, searchBuilder.limit);
        if (items != null)
        {
            packs = packs.OrderByDescending(pack => items.Select((title, index) => (title, index)).FirstOrDefault(tuple => tuple.title == pack.Title).index).ToList();
        }

        return new SearchResults<Modpack>()
        {
            Duration = stopwatch.Elapsed,
            Limit = searchBuilder.limit,
            Offset = searchBuilder.offset,
            Results = packs.Take(searchBuilder.limit).ToArray(),
            TotalResults = totalResults,
        };
    }

    /// <summary>
    /// Searches for resourcepacks based on the provided search criteria.
    /// </summary>
    /// <param name="searchBuilder">The search criteria builder.</param>
    /// <returns>A collection of search results containing resourcepacks.</returns>
    public static async Task<SearchResults<ResourcePack>> SearchResourcepacksAsync(SearchBuilder searchBuilder)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<ResourcePack> packs = new();
        int totalResults = 0;

        if (!searchBuilder.platforms.Any() || searchBuilder.platforms.Contains(Minecraft.Data.PlatformSource.Modrinth))
        {
            using ModrinthClient client = new();
            FacetBuilder builder = new();

            builder.AddProjectTypes(Modrinth.Data.ModrinthProjectTypes.Resourcepack);

            Modrinth.Model.ModrinthSearchResult? result = await client.SearchAsync(new Modrinth.Model.ModrinthSearchQuery()
            {
                Query = searchBuilder.query,
                Facets = builder,
                Limit = 100,
                Offset = searchBuilder.offset,
                Ordering = Modrinth.Data.SearchOrdering.Relevance,
            });

            if (result != null)
            {
                totalResults += result.Value.TotalHits;
                Modrinth.Model.ModrinthSearchResultItem[] hits = result.Value.Hits;
                foreach (var hit in hits)
                {
                    packs.Add(new()
                    {
                        ID = hit.ProjectId,
                        Title = hit.Title,
                        Description = hit.Description,
                        Author = hit.Author,
                        Icon = hit.IconUrl,
                        Banner = hit.FeaturedGallery,
                        Categories = hit.Categories,
                        Downloads = hit.Downloads,
                        Platform = Minecraft.Data.PlatformSource.Modrinth,
                        GameVersions = hit.Versions,
                        Versions = Array.Empty<ResourceItemVersion>(),
                        ReleaseDate = hit.DateCreated,
                        LastUpdated = hit.DateModified,
                        IsDistributionAllowed = true,
                    });
                }
            }
        }
        if (!searchBuilder.platforms.Any() || searchBuilder.platforms.Contains(Minecraft.Data.PlatformSource.Curseforge))
        {
            if (searchBuilder.apiPlatform.ContainsKey(Minecraft.Data.PlatformSource.Curseforge))
            {
                string apiKey = searchBuilder.apiPlatform[Minecraft.Data.PlatformSource.Curseforge];
                using CurseforgeClient client = new(apiKey);
                Curseforge.Model.CurseforgeSearchResult? result = await client.SearchResourcepacksAsync(searchBuilder.query, searchBuilder.minecraftVersions.First().ID);
                if (result != null)
                {
                    totalResults += result.Value.Pagination.TotalCount;
                    foreach (var item in result.Value.Projects)
                    {
                        packs.Add(new()
                        {
                            ID = item.Id.ToString(),
                            Title = item.Name,
                            Author = item.Authors.FirstOrDefault().Name,
                            Description = item.Summary,
                            Icon = item.Logo.ThumbnailUrl,
                            Downloads = item.DownloadCount,
                            LastUpdated = item.DateModified,
                            ReleaseDate = item.DateCreated,
                            Platform = Minecraft.Data.PlatformSource.Curseforge,
                            IsDistributionAllowed = item.AllowModDistribution,
                            Versions = Array.Empty<ResourceItemVersion>(),
                            Banner = "",
                            GameVersions = new string[] { searchBuilder.minecraftVersions.First().ID },
                            Categories = Array.Empty<string>(),
                        });
                    }
                }
            }
        }

        stopwatch.Stop();
        packs = packs.Distinct().ToList();
        FuzzySearchAlgorithm search = new();
        search.Add(packs.Select(i => i.Title).ToArray());
        string[]? items = search.Search(searchBuilder.query, searchBuilder.limit);
        if (items != null)
        {
            packs = packs.OrderByDescending(pack => items.Select((title, index) => (title, index)).FirstOrDefault(tuple => tuple.title == pack.Title).index).ToList();
        }

        return new SearchResults<ResourcePack>()
        {
            Duration = stopwatch.Elapsed,
            Limit = searchBuilder.limit,
            Offset = searchBuilder.offset,
            Results = packs.Take(searchBuilder.limit).ToArray(),
            TotalResults = totalResults,
        };
    }

    /// <summary>
    /// Searches for worlds based on the provided search criteria.
    /// </summary>
    /// <param name="searchBuilder">The search criteria builder.</param>
    /// <returns>A collection of search results containing worlds.</returns>
    public static async Task<SearchResults<World>> SearchWorldsAsync(SearchBuilder searchBuilder)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<World> packs = new();
        int totalResults = 0;

        if (!searchBuilder.platforms.Any() || searchBuilder.platforms.Contains(Minecraft.Data.PlatformSource.Curseforge))
        {
            if (searchBuilder.apiPlatform.ContainsKey(Minecraft.Data.PlatformSource.Curseforge))
            {
                string apiKey = searchBuilder.apiPlatform[Minecraft.Data.PlatformSource.Curseforge];
                using CurseforgeClient client = new(apiKey);
                Curseforge.Model.CurseforgeSearchResult? result = await client.SearchWorldsAsync(searchBuilder.query, searchBuilder.minecraftVersions.First().ID);
                if (result != null)
                {
                    totalResults += result.Value.Pagination.TotalCount;
                    foreach (var item in result.Value.Projects)
                    {
                        packs.Add(new()
                        {
                            ID = item.Id.ToString(),
                            Title = item.Name,
                            Author = item.Authors.FirstOrDefault().Name,
                            Description = item.Summary,
                            Icon = item.Logo.ThumbnailUrl,
                            Downloads = item.DownloadCount,
                            LastUpdated = item.DateModified,
                            ReleaseDate = item.DateCreated,
                            Platform = Minecraft.Data.PlatformSource.Curseforge,
                            IsDistributionAllowed = item.AllowModDistribution,
                            Versions = Array.Empty<ResourceItemVersion>(),
                            Banner = "",
                            GameVersions = new string[] { searchBuilder.minecraftVersions.First().ID },
                            Categories = Array.Empty<string>(),
                        });
                    }
                }
            }
        }

        stopwatch.Stop();
        FuzzySearchAlgorithm search = new();
        search.Add(packs.Select(i => i.Title).ToArray());
        string[]? items = search.Search(searchBuilder.query, searchBuilder.limit);
        if (items != null)
        {
            packs = packs.OrderByDescending(pack => items.Select((title, index) => (title, index)).FirstOrDefault(tuple => tuple.title == pack.Title).index).ToList();
        }

        return new SearchResults<World>()
        {
            Duration = stopwatch.Elapsed,
            Limit = searchBuilder.limit,
            Offset = searchBuilder.offset,
            Results = packs.Take(searchBuilder.limit).ToArray(),
            TotalResults = totalResults,
        };
    }

    /// <summary>
    /// Searches for shaderpacks based on the provided search criteria.
    /// </summary>
    /// <param name="searchBuilder">The search criteria builder.</param>
    /// <returns>A collection of search results containing shaderpacks.</returns>
    public static async Task<SearchResults<ShaderPack>> SearchShaderpacksAsync(SearchBuilder searchBuilder)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<ShaderPack> packs = new();
        int totalResults = 0;

        if (!searchBuilder.platforms.Any() || searchBuilder.platforms.Contains(Minecraft.Data.PlatformSource.Modrinth))
        {
            using ModrinthClient client = new();
            FacetBuilder builder = new();

            builder.AddProjectTypes(Modrinth.Data.ModrinthProjectTypes.Shader);

            Modrinth.Model.ModrinthSearchResult? result = await client.SearchAsync(new Modrinth.Model.ModrinthSearchQuery()
            {
                Query = searchBuilder.query,
                Facets = builder,
                Limit = searchBuilder.limit,
                Offset = searchBuilder.offset,
                Ordering = Modrinth.Data.SearchOrdering.Relevance,
            });

            if (result != null)
            {
                totalResults += result.Value.TotalHits;
                Modrinth.Model.ModrinthSearchResultItem[] hits = result.Value.Hits;
                foreach (var hit in hits)
                {
                    packs.Add(new()
                    {
                        ID = hit.ProjectId,
                        Title = hit.Title,
                        Description = hit.Description,
                        Author = hit.Author,
                        Icon = hit.IconUrl,
                        Banner = hit.FeaturedGallery,
                        Categories = hit.Categories,
                        Downloads = hit.Downloads,
                        Platform = Minecraft.Data.PlatformSource.Modrinth,
                        GameVersions = hit.Versions,
                        Versions = Array.Empty<ResourceItemVersion>(),
                        ReleaseDate = hit.DateCreated,
                        LastUpdated = hit.DateModified,
                        IsDistributionAllowed = true,
                    });
                }
            }
        }
        stopwatch.Stop();

        return new SearchResults<ShaderPack>()
        {
            Duration = stopwatch.Elapsed,
            Limit = searchBuilder.limit,
            Offset = searchBuilder.offset,
            Results = packs.ToArray(),
            TotalResults = totalResults,
        };
    }

    /// <summary>
    /// Searches for datapacks based on the provided search criteria.
    /// </summary>
    /// <param name="searchBuilder">The search criteria builder.</param>
    /// <returns>A collection of search results containing datapacks.</returns>
    public static async Task<SearchResults<Datapack>> SearchDatapacksAsync(SearchBuilder searchBuilder)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<Datapack> packs = new();
        int totalResults = 0;

        if (!searchBuilder.platforms.Any() || searchBuilder.platforms.Contains(Minecraft.Data.PlatformSource.Modrinth))
        {
            using ModrinthClient client = new();
            FacetBuilder builder = new();

            builder.AddCategories("datapack");

            Modrinth.Model.ModrinthSearchResult? result = await client.SearchAsync(new Modrinth.Model.ModrinthSearchQuery()
            {
                Query = searchBuilder.query,
                Facets = builder,
                Limit = searchBuilder.limit,
                Offset = searchBuilder.offset,
                Ordering = Modrinth.Data.SearchOrdering.Relevance,
            });

            if (result != null)
            {
                totalResults += result.Value.TotalHits;
                Modrinth.Model.ModrinthSearchResultItem[] hits = result.Value.Hits;
                foreach (var hit in hits)
                {
                    packs.Add(new()
                    {
                        ID = hit.ProjectId,
                        Title = hit.Title,
                        Description = hit.Description,
                        Author = hit.Author,
                        Icon = hit.IconUrl,
                        Banner = hit.FeaturedGallery,
                        Categories = hit.Categories,
                        Downloads = hit.Downloads,
                        Platform = Minecraft.Data.PlatformSource.Modrinth,
                        GameVersions = hit.Versions,
                        Versions = Array.Empty<ResourceItemVersion>(),
                        ReleaseDate = hit.DateCreated,
                        LastUpdated = hit.DateModified,
                        IsDistributionAllowed = true,
                    });
                }
            }
        }
        stopwatch.Stop();

        return new SearchResults<Datapack>()
        {
            Duration = stopwatch.Elapsed,
            Limit = searchBuilder.limit,
            Offset = searchBuilder.offset,
            Results = packs.ToArray(),
            TotalResults = totalResults,
        };
    }
}