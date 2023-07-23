// LFInteractive LLC. 2021-2024﻿
using System.Text;

namespace Chase.Minecraft.Modrinth;

public class FacetBuilder
{
    private readonly StringBuilder builder;

    public FacetBuilder()
    {
        builder = new();
        builder.Append("facets=[");
    }

    public FacetBuilder AddModloaders(params ModLoaders[] loaders)
    {
        return AddCategories(Array.ConvertAll(loaders, i => i.ToString().ToLower()));
    }

    public FacetBuilder AddCategories(params string[] categories)
    {
        StringBuilder categoryBuilder = new();
        builder.Append('[');
        foreach (string category in categories)
        {
            categoryBuilder.Append("\"categories:");
            categoryBuilder.Append(category);
            categoryBuilder.Append("\",");
        }
        builder.Append(categoryBuilder.ToString().Trim(','));
        builder.Append("\"],");

        return this;
    }

    public FacetBuilder AddVersions(params string[] versions)
    {
        StringBuilder verstionBuilder = new();
        builder.Append('[');
        foreach (string version in versions)
        {
            verstionBuilder.Append("\"versions:");
            verstionBuilder.Append(version);
            verstionBuilder.Append("\",");
        }
        builder.Append(verstionBuilder.ToString().Trim(','));
        builder.Append("\"],");

        return this;
    }

    public FacetBuilder AddLicenses(params string[] licenses)
    {
        StringBuilder licenceBuilder = new();
        builder.Append('[');
        foreach (string license in licenses)
        {
            licenceBuilder.Append("\"license:");
            licenceBuilder.Append(license);
            licenceBuilder.Append("\",");
        }
        builder.Append(licenceBuilder.ToString().Trim(','));
        builder.Append("\"],");

        return this;
    }

    public FacetBuilder AddProjectTypes(params string[] types)
    {
        StringBuilder typeBuilder = new();
        builder.Append('[');
        foreach (string type in types)
        {
            typeBuilder.Append("\"project_type:");
            typeBuilder.Append(type);
            typeBuilder.Append("\",");
        }
        builder.Append(typeBuilder.ToString().Trim(','));
        builder.Append("\"],");

        return this;
    }

    public string Build()
    {
        return builder.ToString().Trim(',') + "]";
    }
}