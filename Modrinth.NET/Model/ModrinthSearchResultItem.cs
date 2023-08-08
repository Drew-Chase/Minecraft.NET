/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Modrinth.Data;
using Newtonsoft.Json;

namespace Chase.Minecraft.Modrinth.Model;

public struct ModrinthSearchResultItem
{
    [JsonProperty("slug")]
    public string Slug { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("categories")]
    public string[] Categories { get; set; }

    [JsonProperty("client_side")]
    public SideRequirements ClientSide { get; set; }

    [JsonProperty("server_side")]
    public SideRequirements ServerSide { get; set; }

    [JsonProperty("project_type")]
    public string ProjectType { get; set; }

    [JsonProperty("downloads")]
    public int Downloads { get; set; }

    [JsonProperty("icon_url")]
    public string IconUrl { get; set; }

    [JsonProperty("color")]
    public long? Color { get; set; }

    [JsonProperty("project_id")]
    public string ProjectId { get; set; }

    [JsonProperty("author")]
    public string Author { get; set; }

    [JsonProperty("display_categories")]
    public string[] DisplayCategories { get; set; }

    [JsonProperty("versions")]
    public string[] Versions { get; set; }

    [JsonProperty("follows")]
    public int Follows { get; set; }

    [JsonProperty("date_created")]
    public DateTime DateCreated { get; set; }

    [JsonProperty("date_modified")]
    public DateTime DateModified { get; set; }

    [JsonProperty("latest_version")]
    public string LatestVersion { get; set; }

    [JsonProperty("license")]
    public string License { get; set; }

    [JsonProperty("gallery")]
    public string[] Gallery { get; set; }

    [JsonProperty("featured_gallery")]
    public string FeaturedGallery { get; set; }
}