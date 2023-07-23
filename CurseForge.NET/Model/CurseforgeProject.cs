// LFInteractive LLC. 2021-2024﻿
using Newtonsoft.Json;

namespace Chase.Minecraft.Curseforge.Model;

using System;

public struct CurseforgeProject
{
    [JsonProperty("id")]
    public int Id { get; set; }

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
    public LatestFile[] LatestFiles { get; set; }

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

public struct Links
{
    [JsonProperty("websiteUrl")]
    public string WebsiteUrl { get; set; }

    [JsonProperty("wikiUrl")]
    public string WikiUrl { get; set; }

    [JsonProperty("issuesUrl")]
    public string IssuesUrl { get; set; }

    [JsonProperty("sourceUrl")]
    public string SourceUrl { get; set; }
}

public struct Author
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}

public struct Logo
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("modId")]
    public int ModId { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("thumbnailUrl")]
    public string ThumbnailUrl { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}

public struct Screenshot
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("modId")]
    public int ModId { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("thumbnailUrl")]
    public string ThumbnailUrl { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}

public struct LatestFile
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("gameId")]
    public int GameId { get; set; }

    [JsonProperty("modId")]
    public int ModId { get; set; }

    [JsonProperty("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonProperty("displayName")]
    public string DisplayName { get; set; }

    [JsonProperty("fileName")]
    public string FileName { get; set; }

    [JsonProperty("releaseType")]
    public int ReleaseType { get; set; }

    [JsonProperty("fileStatus")]
    public int FileStatus { get; set; }

    [JsonProperty("hashes")]
    public FileHash[] Hashes { get; set; }

    [JsonProperty("fileDate")]
    public DateTime FileDate { get; set; }

    [JsonProperty("fileLength")]
    public int FileLength { get; set; }

    [JsonProperty("downloadCount")]
    public int DownloadCount { get; set; }

    [JsonProperty("fileSizeOnDisk")]
    public int FileSizeOnDisk { get; set; }

    [JsonProperty("downloadUrl")]
    public string DownloadUrl { get; set; }

    [JsonProperty("gameVersions")]
    public string[] GameVersions { get; set; }

    [JsonProperty("sortableGameVersions")]
    public SortableGameVersion[] SortableGameVersions { get; set; }

    [JsonProperty("dependencies")]
    public Dependency[] Dependencies { get; set; }

    [JsonProperty("exposeAsAlternative")]
    public bool ExposeAsAlternative { get; set; }

    [JsonProperty("parentProjectFileId")]
    public int ParentProjectFileId { get; set; }

    [JsonProperty("alternateFileId")]
    public int AlternateFileId { get; set; }

    [JsonProperty("isServerPack")]
    public bool IsServerPack { get; set; }

    [JsonProperty("serverPackFileId")]
    public int ServerPackFileId { get; set; }

    [JsonProperty("isEarlyAccessContent")]
    public bool IsEarlyAccessContent { get; set; }

    [JsonProperty("earlyAccessEndDate")]
    public DateTime EarlyAccessEndDate { get; set; }

    [JsonProperty("fileFingerprint")]
    public int FileFingerprint { get; set; }

    [JsonProperty("modules")]
    public Module[] Modules { get; set; }
}

public struct FileHash
{
    [JsonProperty("value")]
    public string Value { get; set; }

    [JsonProperty("algo")]
    public int Algo { get; set; }
}

public struct SortableGameVersion
{
    [JsonProperty("gameVersionName")]
    public string GameVersionName { get; set; }

    [JsonProperty("gameVersionPadded")]
    public string GameVersionPadded { get; set; }

    [JsonProperty("gameVersion")]
    public string GameVersion { get; set; }

    [JsonProperty("gameVersionReleaseDate")]
    public DateTime GameVersionReleaseDate { get; set; }

    [JsonProperty("gameVersionTypeId")]
    public int GameVersionTypeId { get; set; }
}

public struct Dependency
{
    [JsonProperty("modId")]
    public int ModId { get; set; }

    [JsonProperty("relationType")]
    public int RelationType { get; set; }
}

public struct Module
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("fingerprint")]
    public int Fingerprint { get; set; }
}

public struct LatestFileIndex
{
    [JsonProperty("gameVersion")]
    public string GameVersion { get; set; }

    [JsonProperty("fileId")]
    public int FileId { get; set; }

    [JsonProperty("filename")]
    public string Filename { get; set; }

    [JsonProperty("releaseType")]
    public int ReleaseType { get; set; }

    [JsonProperty("gameVersionTypeId")]
    public int GameVersionTypeId { get; set; }

    [JsonProperty("modLoader")]
    public int ModLoader { get; set; }
}