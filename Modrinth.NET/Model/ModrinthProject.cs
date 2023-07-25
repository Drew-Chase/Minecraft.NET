/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Modrinth.Model;

using Chase.Minecraft.Modrinth.Data;
using Newtonsoft.Json;

public struct ModrinthProject
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

    [JsonProperty("body")]
    public string Body { get; set; }

    [JsonProperty("additional_categories")]
    public string[] AdditionalCategories { get; set; }

    [JsonProperty("issues_url")]
    public string IssuesUrl { get; set; }

    [JsonProperty("source_url")]
    public string SourceUrl { get; set; }

    [JsonProperty("wiki_url")]
    public string WikiUrl { get; set; }

    [JsonProperty("discord_url")]
    public string DiscordUrl { get; set; }

    [JsonProperty("donation_urls")]
    public DonationUrl[] DonationUrls { get; set; }

    [JsonProperty("project_type")]
    public string ProjectType { get; set; }

    [JsonProperty("downloads")]
    public int Downloads { get; set; }

    [JsonProperty("icon_url")]
    public string IconUrl { get; set; }

    [JsonProperty("color")]
    public int Color { get; set; }

    [JsonProperty("id")]
    public string ProjectId { get; set; }

    [JsonProperty("team")]
    public string Team { get; set; }

    [JsonProperty("body_url")]
    public string BodyUrl { get; set; }

    [JsonProperty("moderator_message")]
    public string ModeratorMessage { get; set; }

    [JsonProperty("published")]
    public string Published { get; set; }

    [JsonProperty("updated")]
    public string Updated { get; set; }

    [JsonProperty("approved")]
    public string Approved { get; set; }

    [JsonProperty("followers")]
    public int Followers { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("license")]
    public LicenseInfo License { get; set; }

    [JsonProperty("versions")]
    public string[] Versions { get; set; }

    [JsonProperty("game_versions")]
    public string[] GameVersions { get; set; }

    [JsonProperty("loaders")]
    public string[] Loaders { get; set; }

    [JsonProperty("gallery")]
    public GalleryItem[] Gallery { get; set; }
}

public struct DonationUrl
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("platform")]
    public string Platform { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}

public struct LicenseInfo
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}

public struct GalleryItem
{
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("featured")]
    public bool Featured { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("created")]
    public string Created { get; set; }

    [JsonProperty("ordering")]
    public int Ordering { get; set; }
}