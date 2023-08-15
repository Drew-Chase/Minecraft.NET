/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Data;

namespace Chase.Minecraft.Model.Resources;

/// <summary>
/// Represents the base class for resource items.
/// </summary>
public abstract class ResourceItemBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the resource item.
    /// </summary>
    public string ID { get; set; }

    /// <summary>
    /// Gets or sets the title of the resource item.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the author of the resource item.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Gets or sets the description of the resource item.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the number of downloads for the resource item.
    /// </summary>
    public int Downloads { get; set; }

    /// <summary>
    /// Gets or sets the URL of the icon associated with the resource item.
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Gets or sets the URL of the banner associated with the resource item.
    /// </summary>
    public string Banner { get; set; }

    /// <summary>
    /// Gets or sets the versions of the resource item.
    /// </summary>
    public ResourceItemVersion[] Versions { get; set; }

    /// <summary>
    /// Gets or sets the supported game versions for the resource item.
    /// </summary>
    public string[] GameVersions { get; set; }

    /// <summary>
    /// Gets or sets the categories associated with the resource item.
    /// </summary>
    public string[] Categories { get; set; }

    /// <summary>
    /// Gets or sets the release date of the resource item.
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// Gets or sets the last update date of the resource item.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the platform source of the resource item.
    /// </summary>
    public PlatformSource Platform { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether distribution of the resource item is allowed.
    /// </summary>
    public bool IsDistributionAllowed { get; set; }

    /// <summary>
    /// Gets or sets the website URL associated with the resource item.
    /// </summary>
    public string Website { get; set; }
}