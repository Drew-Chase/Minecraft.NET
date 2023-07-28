/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Curseforge.Model;

using System;

public struct CurseforgeProject
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("gameId")]
    public int GameId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("slug")]
    public string Slug { get; set; }

    [JsonProperty("links")]
    public Links Links { get; set; }

    [JsonProperty("summary")]
    public string Summary { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("downloadCount")]
    public int DownloadCount { get; set; }

    [JsonProperty("isFeatured")]
    public bool IsFeatured { get; set; }

    [JsonProperty("primaryCategoryId")]
    public int PrimaryCategoryId { get; set; }

    [JsonProperty("categories")]
    public CurseforgeProject[] Categories { get; set; }

    [JsonProperty("classId")]
    public int ClassId { get; set; }

    [JsonProperty("authors")]
    public Author[] Authors { get; set; }

    [JsonProperty("logo")]
    public Logo Logo { get; set; }

    [JsonProperty("screenshots")]
    public Screenshot[] Screenshots { get; set; }

    [JsonProperty("mainFileId")]
    public int MainFileId { get; set; }

    [JsonProperty("latestFiles")]
    public ModFile[] LatestFiles { get; set; }

    [JsonProperty("latestFilesIndexes")]
    public LatestFileIndex[] LatestFilesIndexes { get; set; }

    [JsonProperty("latestEarlyAccessFilesIndexes")]
    public LatestFileIndex[] LatestEarlyAccessFilesIndexes { get; set; }

    [JsonProperty("dateCreated")]
    public DateTime DateCreated { get; set; }

    [JsonProperty("dateModified")]
    public DateTime DateModified { get; set; }

    [JsonProperty("dateReleased")]
    public DateTime DateReleased { get; set; }

    [JsonProperty("allowModDistribution")]
    public bool AllowModDistribution { get; set; }

    [JsonProperty("gamePopularityRank")]
    public int GamePopularityRank { get; set; }

    [JsonProperty("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonProperty("thumbsUpCount")]
    public int ThumbsUpCount { get; set; }
}