![](/Icons/Modrinth.jpg)
# Modrinth.NET Library

The Modrinth.NET library, developed by LFInteractive LLC, provides a set of functionalities to interact with the Modrinth API. Modrinth is a platform that hosts Minecraft-related content such as mods, modpacks, resource packs, and more. This library allows you to search for mods, fetch project details, download mod files, and retrieve information about users and categories on Modrinth.

## Installation

To use the Modrinth.NET library in your project, you can add it as a NuGet package using the NuGet Package Manager or the Package Manager Console in Visual Studio. The package name is `Modrinth.NET`.
```powershell
Install-Package Modrinth.NET
```

## Usage

The library provides a `ModrinthClient` class to interact with the Modrinth API. Here are some of the key functionalities provided by the library:

1. **Search for Mods:**

You can perform searches for mods based on various criteria using the `ModrinthSearchQuery` and `FacetBuilder` classes.

```csharp
using Chase.Minecraft.Modrinth;
using Chase.Minecraft.Modrinth.Data;

FacetBuilder builder = new FacetBuilder()
   .AddVersions("1.19.4")
   .AddProjectTypes(ModrinthProjectTypes.Mod)
   .AddModloaders(Chase.Minecraft.ModLoaders.Fabric);

ModrinthSearchQuery query = new ModrinthSearchQuery()
{
    Facets = builder,
    Query = "The Warp Mod",
    Limit = 2,
    Ordering = SearchOrdering.Relevance,
};

using ModrinthClient client = new ModrinthClient();
ModrinthSearchResult? results = await client.SearchAsync(query);
```

2. **Retrieve Project Details:**

You can fetch details about a specific project, such as versions, dependencies, and files.

```csharp
ModrinthSearchResultItem topResult = results.Value.Hits[0];
ModrinthVersionFile[]? projectVersions = await client.GetProjectVersionsAsync(topResult.ProjectId);
string filePath = await client.DownloadVersionFile(projectVersions[0].Files[0], instance, "mods");
ModrinthProjectDependencies? dependencies = await client.GetProjectDependenciesAsync(topResult.ProjectId);
```

3. **Retrieve User Information:**

You can get information about a Modrinth user based on their username and access their projects.

```csharp
ModrinthUser? user = await client.GetUserAsync("dcmanproductions");
ModrinthProject[] projects = user?.Projects ?? Array.Empty<ModrinthProject>();
```

4. **Retrieve Category Information:**

You can obtain a list of all Modrinth categories.

```csharp
CategoryTag[]? categories = await client.GetCategoriesAsync();
```


## FacetBuilder

The `FacetBuilder` class is a utility class within the Modrinth.NET library, responsible for building facets to be used in Modrinth's Search API. Facets are used to filter search results based on specific criteria, such as modloaders, categories, versions, licenses, and project types.

## Constructors

### `public FacetBuilder()`

Initializes a new instance of the `FacetBuilder` class.

## Properties

### `internal bool IsEmpty { get; private set; }`

Gets a value indicating whether the facet builder is empty or not. An empty facet builder means that no facets have been added yet.

## Methods

### `public FacetBuilder AddModloaders(params ModLoaders[] loaders)`

Adds a facet for modloaders to the facet builder. You can add multiple modloaders in one call, which will be considered an 'OR' request. Alternatively, you can add them individually for an 'AND' request.

**Parameters:**
- `loaders`: A list of Minecraft mod loaders represented by the `ModLoaders` enumeration.

### `public FacetBuilder AddCategories(params string[] categories)`

Adds a facet for categories to the facet builder. You can add multiple categories in one call, which will be considered an 'OR' request. Alternatively, you can add them individually for an 'AND' request.

**Parameters:**
- `categories`: A list of categories represented by strings.

### `public FacetBuilder AddVersions(params string[] versions)`

Adds a facet for Minecraft versions to the facet builder. You can add multiple versions in one call, which will be considered an 'OR' request. Alternatively, you can add them individually for an 'AND' request.

**Parameters:**
- `versions`: A list of Minecraft versions represented by strings.

### `public FacetBuilder AddLicenses(params string[] licenses)`

Adds a facet for licenses to the facet builder. You can add multiple licenses in one call, which will be considered an 'OR' request. Alternatively, you can add them individually for an 'AND' request.

**Parameters:**
- `licenses`: A list of license types represented by strings.

### `public FacetBuilder AddProjectTypes(params ModrinthProjectTypes[] types)`

Adds a facet for project types to the facet builder. You can add multiple project types in one call, which will be considered an 'OR' request. Alternatively, you can add them individually for an 'AND' request.

**Parameters:**
- `types`: A list of project types represented by the `ModrinthProjectTypes` enumeration.

### `public FacetBuilder AddProjectTypes(params string[] types)`

Adds a facet for project types to the facet builder. You can add multiple project types in one call, which will be considered an 'OR' request. Alternatively, you can add them individually for an 'AND' request.

**Parameters:**
- `types`: A list of project types represented by strings.

### `public string Build()`

Builds and returns the finalized string output of the facet builder. The built string can be used in Modrinth's Search API to filter search results based on the specified facets.

**Returns:** A string representing the built facets.

### `public override string ToString()`

Overrides the `ToString()` method and returns the same output as the `Build()` method. This is for convenience when converting the facet builder to a string representation.

**Returns:** A string representing the built facets.

## Example:
```csharp
// Example 1: Building a facet for modloaders (OR request)
FacetBuilder facetBuilder = new FacetBuilder();
facetBuilder.AddModloaders(ModLoaders.Fabric, ModLoaders.Forge);
string facet1 = facetBuilder.Build();
Console.WriteLine("Facet 1:");
Console.WriteLine(facet1);

// Example 2: Building a facet for categories (AND request)
facetBuilder = new FacetBuilder();
facetBuilder.AddCategories("utilities", "world-gen");
string facet2 = facetBuilder.Build();
Console.WriteLine("\nFacet 2:");
Console.WriteLine(facet2);

// Example 3: Building a facet for Minecraft versions (OR request)
facetBuilder = new FacetBuilder();
facetBuilder.AddVersions("1.16.5", "1.17.1");
string facet3 = facetBuilder.Build();
Console.WriteLine("\nFacet 3:");
Console.WriteLine(facet3);

// Example 4: Building a facet for licenses (AND request)
facetBuilder = new FacetBuilder();
facetBuilder.AddLicenses("MIT", "GPL-3.0");
string facet4 = facetBuilder.Build();
Console.WriteLine("\nFacet 4:");
Console.WriteLine(facet4);

// Example 5: Building a facet for project types (OR request)
facetBuilder = new FacetBuilder();
facetBuilder.AddProjectTypes(ModrinthProjectTypes.Mod, ModrinthProjectTypes.Modpack);
string facet5 = facetBuilder.Build();
Console.WriteLine("\nFacet 5:");
Console.WriteLine(facet5);
```


## License

The Modrinth.NET library is licensed under the GNU General Public License (GPL) version 3.0. For detailed information about the license terms, please visit the [GPL-3.0 License Text](https://www.gnu.org/licenses/gpl-3.0.en.html#license-text) page.

Please note that the Modrinth.NET library interacts with Modrinth's API, and you may need to comply with Modrinth's terms of use and API usage policies when integrating it into your applications. For further details on how to use the Modrinth.NET library and its functionalities, refer to the links provided in the documentation and the official Modrinth API documentation.
