/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Curseforge.Controller;
using Chase.Minecraft.Data;
using Chase.Minecraft.Model.Resources;
using Chase.Minecraft.Modrinth;
using Chase.Minecraft.Modrinth.Controller;
using Chase.Minecraft.Modrinth.Data;
using Chase.Minecraft.Modrinth.Model;
using SearchNET.Algorithm;
using System.Diagnostics;

namespace Chase.Minecraft.Modpacks;

/// <summary>
/// Provides methods to search for Minecraft mods, modpacks, resourcepacks, worlds, shaderpacks, and datapacks.
/// </summary>
public static class MultiPlatformInteropClient
{
    private static FuzzySearchAlgorithm search = new(int.MaxValue);

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

            if (searchBuilder.minecraftVersions.Any())
            {
                builder.AddVersions(searchBuilder.minecraftVersions.Select(i => i.ID).ToArray());
            }

            if (searchBuilder.loaders.Any())
            {
                builder.AddModloaders(searchBuilder.loaders.ToArray());
            }

            builder.AddProjectTypes(ProjectTypes.Mod);

            Modrinth.Model.ModrinthSearchResult? result = await client.SearchAsync(new Modrinth.Model.ModrinthSearchQuery()
            {
                Query = searchBuilder.query,
                Facets = builder,
                Limit = 100,
                Offset = searchBuilder.offset,
                Ordering = SearchOrdering.Relevance,
            });

            if (result != null)
            {
                totalResults += result.Value.TotalHits;
                Modrinth.Model.ModrinthSearchResultItem[] hits = result.Value.Hits;
                foreach (var item in hits)
                {
                    mods.Add(new()
                    {
                        ID = item.ProjectId,
                        Title = item.Title,
                        Description = item.Description,
                        Author = item.Author,
                        Icon = item.IconUrl,
                        Banner = item.FeaturedGallery,
                        Categories = item.Categories,
                        ClientSide = item.ClientSide,
                        ServerSide = item.ServerSide,
                        Downloads = item.Downloads,
                        Platform = Minecraft.Data.PlatformSource.Modrinth,
                        GameVersions = item.Versions,
                        Versions = Array.Empty<ResourceItemVersion>(),
                        SupportedLoaders = searchBuilder.loaders.ToArray(),
                        ReleaseDate = item.DateCreated,
                        LastUpdated = item.DateModified,
                        IsDistributionAllowed = true,
                        Website = $"https://modrinth.com/mod/{item.Slug}"
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
                        Mod? mod = mods.First(i => i.Title == item.Name && i.Author == item.Authors.First().Name) ?? null;
                        if (mod != null)
                        {
                            mods.Add(new()
                            {
                                ID = item.Id.ToString(),
                                Title = item.Name,
                                Author = item.Authors.FirstOrDefault().Name,
                                ClientSide = SideRequirements.Required,
                                ServerSide = SideRequirements.Required,
                                Description = item.Summary,
                                Icon = item.Logo.ThumbnailUrl,
                                Downloads = item.DownloadCount,
                                LastUpdated = item.DateModified,
                                ReleaseDate = item.DateCreated,
                                Platform = Minecraft.Data.PlatformSource.Curseforge,
                                IsDistributionAllowed = item.AllowModDistribution,
                                Versions = Array.Empty<ResourceItemVersion>(),
                                SupportedLoaders = searchBuilder.loaders.ToArray(),
                                Banner = "",
                                GameVersions = new string[] { searchBuilder.minecraftVersions.First().ID },
                                Categories = Array.Empty<string>(),
                                Website = $"https://curseforge.com/minecraft/mc-mods/{item.Slug}"
                            });
                        }
                    }
                }
            }
        }

        stopwatch.Stop();
        if (!string.IsNullOrWhiteSpace(searchBuilder.query))
        {
            search.Add(mods.Select(i => i.Title).ToArray());
            string[]? items = search.Search(searchBuilder.query, searchBuilder.limit);
            if (items != null)
            {
                mods = mods.OrderByDescending(mod => items.Select((title, index) => (title, index)).FirstOrDefault(tuple => tuple.title == mod.Title).index).ToList();
            }
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

            if (searchBuilder.minecraftVersions.Any())
            {
                builder.AddVersions(searchBuilder.minecraftVersions.Select(i => i.ID).ToArray());
            }

            if (searchBuilder.loaders.Any())
            {
                builder.AddModloaders(searchBuilder.loaders.ToArray());
            }

            builder.AddProjectTypes(ProjectTypes.Modpack);

            Modrinth.Model.ModrinthSearchResult? result = await client.SearchAsync(new Modrinth.Model.ModrinthSearchQuery()
            {
                Query = searchBuilder.query,
                Facets = builder,
                Limit = 100,
                Offset = searchBuilder.offset,
                Ordering = SearchOrdering.Relevance,
            });

            if (result != null)
            {
                totalResults += result.Value.TotalHits;
                Modrinth.Model.ModrinthSearchResultItem[] hits = result.Value.Hits;
                foreach (var item in hits)
                {
                    packs.Add(new()
                    {
                        ID = item.ProjectId,
                        Title = item.Title,
                        Description = item.Description,
                        Author = item.Author,
                        Icon = item.IconUrl,
                        Banner = item.FeaturedGallery,
                        Categories = item.Categories,
                        Downloads = item.Downloads,
                        Platform = Minecraft.Data.PlatformSource.Modrinth,
                        GameVersions = item.Versions,
                        Versions = Array.Empty<ResourceItemVersion>(),
                        SupportedLoaders = Array.Empty<ModLoaders>(),
                        ReleaseDate = item.DateCreated,
                        LastUpdated = item.DateModified,
                        IsDistributionAllowed = true,
                        Website = $"https://modrinth.com/modpack/{item.Slug}"
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
                            Website = $"https://curseforge.com.com/minecraft/modpacks/{item.Slug}"
                        });
                    }
                }
            }
        }

        stopwatch.Stop();
        packs = packs.Distinct().ToList();
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

            builder.AddProjectTypes(ProjectTypes.Resourcepack);

            Modrinth.Model.ModrinthSearchResult? result = await client.SearchAsync(new ModrinthSearchQuery()
            {
                Query = searchBuilder.query,
                Facets = builder,
                Limit = 100,
                Offset = searchBuilder.offset,
                Ordering = SearchOrdering.Relevance,
            });

            if (result != null)
            {
                totalResults += result.Value.TotalHits;
                Modrinth.Model.ModrinthSearchResultItem[] hits = result.Value.Hits;
                foreach (var item in hits)
                {
                    packs.Add(new()
                    {
                        ID = item.ProjectId,
                        Title = item.Title,
                        Description = item.Description,
                        Author = item.Author,
                        Icon = item.IconUrl,
                        Banner = item.FeaturedGallery,
                        Categories = item.Categories,
                        Downloads = item.Downloads,
                        Platform = Minecraft.Data.PlatformSource.Modrinth,
                        GameVersions = item.Versions,
                        Versions = Array.Empty<ResourceItemVersion>(),
                        ReleaseDate = item.DateCreated,
                        LastUpdated = item.DateModified,
                        IsDistributionAllowed = true,
                        Website = $"https://modrinth.com/resourcepack/{item.Slug}"
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
                            Website = $"https://curseforge.com/minecraft/texture-packs/{item.Slug}"
                        });
                    }
                }
            }
        }

        stopwatch.Stop();
        packs = packs.Distinct().ToList();
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
                            Website = $"https://curseforge.com/minecraft/worlds/{item.Slug}"
                        });
                    }
                }
            }
        }

        stopwatch.Stop();
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

            builder.AddProjectTypes(ProjectTypes.Shader);

            ModrinthSearchResult? result = await client.SearchAsync(new ModrinthSearchQuery()
            {
                Query = searchBuilder.query,
                Facets = builder,
                Limit = searchBuilder.limit,
                Offset = searchBuilder.offset,
                Ordering = SearchOrdering.Relevance,
            });

            if (result != null)
            {
                totalResults += result.Value.TotalHits;
                ModrinthSearchResultItem[] hits = result.Value.Hits;
                foreach (var item in hits)
                {
                    packs.Add(new()
                    {
                        ID = item.ProjectId,
                        Title = item.Title,
                        Description = item.Description,
                        Author = item.Author,
                        Icon = item.IconUrl,
                        Banner = item.FeaturedGallery,
                        Categories = item.Categories,
                        Downloads = item.Downloads,
                        Platform = Minecraft.Data.PlatformSource.Modrinth,
                        GameVersions = item.Versions,
                        Versions = Array.Empty<ResourceItemVersion>(),
                        ReleaseDate = item.DateCreated,
                        LastUpdated = item.DateModified,
                        IsDistributionAllowed = true,
                        Website = $"https://modrinth.com/shader/{item.Slug}"
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

        if (!searchBuilder.platforms.Any() || searchBuilder.platforms.Contains(PlatformSource.Modrinth))
        {
            using ModrinthClient client = new();
            FacetBuilder builder = new();

            builder.AddCategories("datapack");

            Modrinth.Model.ModrinthSearchResult? result = await client.SearchAsync(new ModrinthSearchQuery()
            {
                Query = searchBuilder.query,
                Facets = builder,
                Limit = searchBuilder.limit,
                Offset = searchBuilder.offset,
                Ordering = SearchOrdering.Relevance,
            });

            if (result != null)
            {
                totalResults += result.Value.TotalHits;
                Modrinth.Model.ModrinthSearchResultItem[] hits = result.Value.Hits;
                foreach (var item in hits)
                {
                    packs.Add(new()
                    {
                        ID = item.ProjectId,
                        Title = item.Title,
                        Description = item.Description,
                        Author = item.Author,
                        Icon = item.IconUrl,
                        Banner = item.FeaturedGallery,
                        Categories = item.Categories,
                        Downloads = item.Downloads,
                        Platform = Minecraft.Data.PlatformSource.Modrinth,
                        GameVersions = item.Versions,
                        Versions = Array.Empty<ResourceItemVersion>(),
                        ReleaseDate = item.DateCreated,
                        LastUpdated = item.DateModified,
                        IsDistributionAllowed = true,
                        Website = $"https://modrinth.com/datapack/{item.Slug}"
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