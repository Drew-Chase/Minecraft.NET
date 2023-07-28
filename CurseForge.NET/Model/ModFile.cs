/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Curseforge.Model;

using System;

public struct ModFile
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("gameId")]
    public long GameId { get; set; }

    [JsonProperty("modId")]
    public long ModId { get; set; }

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
    public long FileLength { get; set; }

    [JsonProperty("downloadCount")]
    public int DownloadCount { get; set; }

    [JsonProperty("fileSizeOnDisk")]
    public long FileSizeOnDisk { get; set; }

    [JsonProperty("downloadUrl")]
    public Uri DownloadUrl { get; set; }

    [JsonProperty("gameVersions")]
    public string[] GameVersions { get; set; }

    [JsonProperty("sortableGameVersions")]
    public SortableGameVersion[] SortableGameVersions { get; set; }

    [JsonProperty("dependencies")]
    public Dependency[] Dependencies { get; set; }

    [JsonProperty("exposeAsAlternative")]
    public bool ExposeAsAlternative { get; set; }

    [JsonProperty("parentProjectFileId")]
    public long ParentProjectFileId { get; set; }

    [JsonProperty("alternateFileId")]
    public long AlternateFileId { get; set; }

    [JsonProperty("isServerPack")]
    public bool IsServerPack { get; set; }

    [JsonProperty("serverPackFileId")]
    public long ServerPackFileId { get; set; }

    [JsonProperty("isEarlyAccessContent")]
    public bool IsEarlyAccessContent { get; set; }

    [JsonProperty("earlyAccessEndDate")]
    public DateTime EarlyAccessEndDate { get; set; }

    [JsonProperty("fileFingerprint")]
    public long FileFingerprint { get; set; }

    [JsonProperty("modules")]
    public Module[] Modules { get; set; }
}