// LFInteractive LLC. 2021-2024﻿
using System.Text;
using Chase.Minecraft.Modrinth.Data;

namespace Chase.Minecraft.Modrinth;

public class FacetBuilder
{
    private readonly StringBuilder builder;

    internal bool IsEmpty { get; private set; }

    public FacetBuilder()
    {
        builder = new();
        builder.Append("facets=[");
        IsEmpty = true;
    }

    /// <summary>
    /// Adds a facet for modloaders. If you add them all in one it will be considered an 'OR', or
    /// you can add them with individual function calls for an 'AND' request <br/><a
    /// href="https://docs.modrinth.com/docs/tutorials/api_search/#or">Modrinths Search API Documentation</a>
    /// </summary>
    /// <param name="loaders">A list of minecraft mod loaders</param>
    /// <returns></returns>
    public FacetBuilder AddModloaders(params ModLoaders[] loaders) => AddCategories(Array.ConvertAll(loaders, i => i.ToString().ToLower()));

    /// <summary>
    /// Adds a facet for categories. If you add them all in one it will be considered an 'OR', or
    /// you can add them with individual function calls for an 'AND' request <br/><a
    /// href="https://docs.modrinth.com/docs/tutorials/api_search/#or">Modrinths Search API Documentation</a>
    /// </summary>
    /// <param name="categories">A list of modrinth categories</param>
    /// <returns></returns>
    public FacetBuilder AddCategories(params string[] categories)
    {
        IsEmpty = false;
        StringBuilder categoryBuilder = new();
        builder.Append('[');
        foreach (string category in categories)
        {
            categoryBuilder.Append("\"categories:");
            categoryBuilder.Append(category);
            categoryBuilder.Append("\",");
        }
        builder.Append(categoryBuilder.ToString().Trim(','));
        builder.Append("],");

        return this;
    }

    /// <summary>
    /// Adds a facet for versions. If you add them all in one it will be considered an 'OR', or you
    /// can add them with individual function calls for an 'AND' request <br/><a
    /// href="https://docs.modrinth.com/docs/tutorials/api_search/#or">Modrinths Search API Documentation</a>
    /// </summary>
    /// <param name="versions">A list of minecraft versions</param>
    /// <returns></returns>
    public FacetBuilder AddVersions(params string[] versions)
    {
        IsEmpty = false;
        StringBuilder verstionBuilder = new();
        builder.Append('[');
        foreach (string version in versions)
        {
            verstionBuilder.Append("\"versions:");
            verstionBuilder.Append(version);
            verstionBuilder.Append("\",");
        }
        builder.Append(verstionBuilder.ToString().Trim(','));
        builder.Append("],");

        return this;
    }

    /// <summary>
    /// Adds a facet for licenses. If you add them all in one it will be considered an 'OR', or you
    /// can add them with individual function calls for an 'AND' request <br/><a
    /// href="https://docs.modrinth.com/docs/tutorials/api_search/#or">Modrinths Search API Documentation</a>
    /// </summary>
    /// <param name="licenses">A list of license types</param>
    /// <returns></returns>
    public FacetBuilder AddLicenses(params string[] licenses)
    {
        IsEmpty = false;
        StringBuilder licenceBuilder = new();
        builder.Append('[');
        foreach (string license in licenses)
        {
            licenceBuilder.Append("\"license:");
            licenceBuilder.Append(license);
            licenceBuilder.Append("\",");
        }
        builder.Append(licenceBuilder.ToString().Trim(','));
        builder.Append("],");

        return this;
    }

    /// <summary>
    /// Adds a facet for project types. If you add them all in one it will be considered an 'OR', or
    /// you can add them with individual function calls for an 'AND' request <br/><a
    /// href="https://docs.modrinth.com/docs/tutorials/api_search/#or">Modrinths Search API Documentation</a>
    /// </summary>
    /// <param name="types">A list of project types. Ex: mod, modpack, resourcepack, etc</param>
    /// <returns></returns>
    public FacetBuilder AddProjectTypes(params ModrinthProjectTypes[] types) => AddProjectTypes(Array.ConvertAll(types, i => i.ToString().ToLower()));

    /// <summary>
    /// Adds a facet for project types. If you add them all in one it will be considered an 'OR', or
    /// you can add them with individual function calls for an 'AND' request <br/><a
    /// href="https://docs.modrinth.com/docs/tutorials/api_search/#or">Modrinths Search API Documentation</a>
    /// </summary>
    /// <param name="types">A list of project types. Ex: mod, modpack, resourcepack, etc</param>
    /// <returns></returns>
    public FacetBuilder AddProjectTypes(params string[] types)
    {
        IsEmpty = false;
        StringBuilder typeBuilder = new();
        builder.Append('[');
        foreach (string type in types)
        {
            typeBuilder.Append("\"project_type:");
            typeBuilder.Append(type);
            typeBuilder.Append("\",");
        }
        builder.Append(typeBuilder.ToString().Trim(','));
        builder.Append("],");

        return this;
    }

    /// <summary>
    /// Returns the finalized string output.
    /// </summary>
    /// <returns></returns>
    public string Build()
    {
        return builder.ToString().Trim(',') + "]";
    }

    /// <summary>
    /// Returns the finalized string output. <br/> See: <seealso cref="Build">Build()</seealso>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Build();
    }
}