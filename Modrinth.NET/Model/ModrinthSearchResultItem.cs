// LFInteractive LLC. 2021-2024﻿
namespace Chase.Minecraft.Modrinth.Model;

public struct ModrinthSearchResultItem
{
    public string Slug { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string[] Categories { get; set; }
    public SideRequirements ClientSide { get; set; }
    public SideRequirements ServerSide { get; set; }
    public string ProjectType { get; set; }
    public int Downloads { get; set; }
    public string IconUrl { get; set; }
    public int Color { get; set; }
    public string ProjectId { get; set; }
    public string Author { get; set; }
    public string[] DisplayCategories { get; set; }
    public string[] Versions { get; set; }
    public int Follows { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
    public string LatestVersion { get; set; }
    public string License { get; set; }
    public string[] Gallery { get; set; }
    public string FeaturedGallery { get; set; }
}