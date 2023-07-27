# CurseForge.NET Library

The CurseForge.NET library provides functionalities to interact with the CurseForge API, enabling developers to search for and retrieve information about modpacks, mods, worlds, and resource packs hosted on CurseForge. This library facilitates seamless integration with the CurseForge platform for Minecraft-related projects.

## Installation

The CurseForge.NET library can be installed via NuGet Package Manager:

```powershell
Install-Package CurseForge.NET
```

## Initialization

Before using the CurseForge.NET library, you need to initialize the `CurseforgeClient` with your API key.

## `CurseforgeClient(string apiKey)`

Initializes a new instance of the `CurseforgeClient` class with the provided CurseForge API key.

**Parameters:**
- `apiKey`: The API key obtained from CurseForge to access their API.

## Search Methods

The CurseForge.NET library offers various search methods to find modpacks, mods, worlds, and resource packs on CurseForge.

## `Task<CurseforgeSearchResult?> SearchModpackAsync(string query, string version, ModLoaders modLoader)`

Searches for modpacks on CurseForge based on the specified query, version, and modloader.

**Parameters:**
- `query`: The search query for modpacks.
- `version`: The Minecraft version for which to find modpacks.
- `modLoader`: The modloader type represented by the `ModLoaders` enumeration.

**Returns:** A `CurseforgeSearchResult` object containing search results for modpacks.

## `Task<CurseforgeSearchResult?> SearchModsAsync(string query, string version, ModLoaders modLoader)`

Searches for mods on CurseForge based on the specified query, version, and modloader.

**Parameters:**
- `query`: The search query for mods.
- `version`: The Minecraft version for which to find mods.
- `modLoader`: The modloader type represented by the `ModLoaders` enumeration.

**Returns:** A `CurseforgeSearchResult` object containing search results for mods.

## `Task<CurseforgeSearchResult?> SearchWorldsAsync(string query, string version)`

Searches for worlds on CurseForge based on the specified query and version.

**Parameters:**
- `query`: The search query for worlds.
- `version`: The Minecraft version for which to find worlds.

**Returns:** A `CurseforgeSearchResult` object containing search results for worlds.

## `Task<CurseforgeSearchResult?> SearchResourcepacksAsync(string query, string version)`

Searches for resource packs on CurseForge based on the specified query and version.

**Parameters:**
- `query`: The search query for resource packs.
- `version`: The Minecraft version for which to find resource packs.

**Returns:** A `CurseforgeSearchResult` object containing search results for resource packs.

## CurseforgeSearchResult Class

The `CurseforgeSearchResult` class represents the search results returned by the CurseForge API.

## Properties

- `int Total`: Gets the total number of search results.
- `IEnumerable<CurseforgeSearchEntry> Results`: Gets a collection of `CurseforgeSearchEntry` objects representing individual search results.

## CurseforgeSearchEntry Class

The `CurseforgeSearchEntry` class represents an individual search result entry returned by the CurseForge API.

## Properties

- `int Id`: Gets the unique ID of the search result entry.
- `string Name`: Gets the name of the search result entry.
- `string Author`: Gets the author's name of the search result entry.
- `string Summary`: Gets a brief summary of the search result entry.
- `string[] Tags`: Gets an array of tags associated with the search result entry.
- `string GameVersion`: Gets the Minecraft version compatible with the search result entry.
- `string Type`: Gets the type of the search result entry (modpack, mod, world, resourcepack, etc).

## Usage Example

```csharp
using CurseForge.NET;
using Chase.Minecraft.Modrinth.Data;

public class Example
{
    public static async Task Main()
    {
        // Initialize CurseForge client with API key
        using CurseforgeClient client = new CurseforgeClient("api-key");

        // Search for modpacks
        CurseforgeSearchResult? modpacks = await client.SearchModpackAsync("Skyblock", "1.19.4", ModLoaders.Fabric);

        // Search for mods
        CurseforgeSearchResult? mods = await client.SearchModsAsync("Warp", "1.19.4", ModLoaders.Fabric);

        // Search for worlds
        CurseforgeSearchResult? worlds = await client.SearchWorldsAsync("OneBlock", "1.19.4");

        // Search for resource packs
        CurseforgeSearchResult? resourcepacks = await client.SearchResourcepacksAsync("Faithful", "1.19.4");
    }
}
```

In this example, we demonstrate how to use the CurseForge.NET library to search for modpacks, mods, worlds, and resource packs on CurseForge. The `CurseforgeClient` is initialized with the API key, and then various search methods are called to retrieve search results based on different criteria. The results are stored in `CurseforgeSearchResult` objects, which contain individual `CurseforgeSearchEntry` representing each search result entry.

Please note that you need to replace `"api-key"` with your actual CurseForge API key to make successful API calls. Additionally, the search query, Minecraft version, and modloader can be adjusted according to your specific requirements.