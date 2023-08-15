/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Model;
using Newtonsoft.Json;

namespace Chase.Minecraft.Modrinth.Model;

public struct ModrinthVersionFile
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("version_number")]
    public string VersionNumber { get; set; }

    [JsonProperty("changelog")]
    public string Changelog { get; set; }

    [JsonProperty("dependencies")]
    public Dependency[] Dependencies { get; set; }

    [JsonProperty("game_versions")]
    public string[] GameVersions { get; set; }

    [JsonProperty("version_type")]
    public string VersionType { get; set; }

    [JsonProperty("loaders")]
    public string[] Loaders { get; set; }

    [JsonProperty("featured")]
    public bool Featured { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("requested_status")]
    public string RequestedStatus { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("project_id")]
    public string ProjectId { get; set; }

    [JsonProperty("author_id")]
    public string AuthorId { get; set; }

    [JsonProperty("date_published")]
    public DateTime DatePublished { get; set; }

    [JsonProperty("downloads")]
    public int Downloads { get; set; }

    [JsonProperty("changelog_url")]
    public string ChangelogUrl { get; set; }

    [JsonProperty("files")]
    public VersionFileDetails[] Files { get; set; }

    /// <summary>
    /// Converts Modrinth Version File to Instance Mod Model
    /// </summary>
    /// <returns>Instance Mod Model</returns>
    public ModModel ToInstanceMod()
    {
        VersionFileDetails primary = Files.FirstOrDefault(i => i.Primary);
        return new ModModel()
        {
            ProjectID = ProjectId,
            VersionID = Id,
            Source = Minecraft.Data.PlatformSource.Modrinth,
            Name = Name,
            FileName = primary.Filename,
            DownloadURL = primary.Url
        };
    }
}

public struct Dependency
{
    [JsonProperty("version_id")]
    public string VersionId { get; set; }

    [JsonProperty("project_id")]
    public string ProjectId { get; set; }

    [JsonProperty("file_name")]
    public string FileName { get; set; }

    [JsonProperty("dependency_type")]
    public string DependencyType { get; set; }
}

public struct VersionFileDetails
{
    public Hashes hashes { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("filename")]
    public string Filename { get; set; }

    [JsonProperty("primary")]
    public bool Primary { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("file_type")]
    public string FileType { get; set; }
}

public struct Hashes
{
    [JsonProperty("sha512")]
    public string Sha512 { get; set; }

    [JsonProperty("sha1")]
    public string Sha1 { get; set; }
}