![](/Icons/CurseForge.jpg)
# CurseForge.NET Library

The CurseForge.NET library is a client library developed to interact with the CurseForge API, allowing users to search for and access information about Minecraft mods, modpacks, resource packs, and worlds hosted on CurseForge. This library provides a convenient way to access and download files from CurseForge projects.

## Getting Started

To use the CurseForge.NET library, you need to create an instance of the `CurseforgeClient` class and pass your API key as a parameter to authenticate with the CurseForge API.

```csharp
using CurseForge.NET;

// Creates a CurseForge Client with an API-Key
using CurseforgeClient client = new CurseforgeClient("YOUR_API_KEY");
```

## Searching for Projects

You can search for various types of projects, such as mods, modpacks, resource packs, and worlds, using the `CurseforgeClient`'s search methods.

### Searching for Modpacks

```csharp
CurseforgeSearchResult? modpacks = await client.SearchModpackAsync("All the Mods 6", "1.16.5", Chase.Minecraft.ModLoaders.Forge);
```

### Searching for Mods

```csharp
CurseforgeSearchResult? mods = await client.SearchModsAsync("Warp", "1.19.4", Chase.Minecraft.ModLoaders.Fabric);
```

### Searching for Worlds

```csharp
CurseforgeSearchResult? worlds = await client.SearchWorldsAsync("OneBlock ", "1.19.4");
```

### Searching for Resource Packs

```csharp
CurseforgeSearchResult? resourcepacks = await client.SearchResourcepacksAsync("Faithful", "1.19.4");
```

## Getting Individual Projects

You can retrieve information about individual projects by their project ID using the corresponding `Get` methods.

### Get Mod Information

```csharp
CurseforgeProject? mod = await client.GetMod("887168");
```

### Get Modpack Information

```csharp
CurseforgeProject? modpack = await client.GetModpack("887168");
```

### Get Resource Pack Information

```csharp
CurseforgeProject? resourcepack = await client.GetResourcepack("887168");
```

### Get World Information

```csharp
CurseforgeProject? world = await client.GetWorld("887168");
```

## Getting Project Files

You can retrieve a list of files associated with a project using the corresponding `GetFiles` methods.

### Get Mod Files

```csharp
ModFile[]? modFiles = await client.GetModFiles("887168");
```

### Get Modpack Files

```csharp
ModFile[]? modpackFiles = await client.GetModpackFiles("887168");
```

### Get Resource Pack Files

```csharp
ModFile[]? resourcepackFiles = await client.GetResourcepackFiles("887168");
```

### Get World Files

```csharp
ModFile[]? worldFiles = await client.GetWorldFiles("887168");
```

## Getting Specific Project Files

You can retrieve information about a specific file associated with a project using the corresponding `GetFile` methods.

### Get Mod File Information

```csharp
ModFile? modFile = await client.GetModFile("887168", "250");
```

### Get Modpack File Information

```csharp
ModFile? modpackFile = await client.GetModpackFile("887168", "250");
```

### Get Resource Pack File Information

```csharp
ModFile? resourcepackFile = await client.GetResourcepackFile("887168", "250");
```

### Get World File Information

```csharp
ModFile? worldFile = await client.GetWorldFile("887168", "250");
```

## Downloading Project Files

You can download project files using the `Download` methods.

### Downloading Mod File

```csharp
await client.Download(modFile.Value, instance, "mods");
```

### Downloading Modpack File

```csharp
await client.Download(modpackFile.Value, "/path/to/modpacks/");
```

### Downloading Resource Pack File

```csharp
await client.Download(resourcepackFile.Value, instance, "resourcepack");
```

### Downloading World File

```csharp
await client.Download(worldFile.Value, instance, "saves");
```

## Example
```csharp
using CurseForge.NET;
using CurseForge.NET.Model;
using System;
using System.IO;

public class Example
{
    public static async Task Main()
    {
        // Creates a CurseForge Client with an API-Key
        using CurseforgeClient client = new CurseforgeClient("YOUR_API_KEY");

        // Search for projects
        CurseforgeSearchResult? modpacks = await client.SearchModpackAsync("All the Mods 6", "1.16.5", Chase.Minecraft.ModLoaders.Forge);
        CurseforgeSearchResult? mods = await client.SearchModsAsync("Warp", "1.19.4", Chase.Minecraft.ModLoaders.Fabric);
        CurseforgeSearchResult? worlds = await client.SearchWorldsAsync("OneBlock", "1.19.4");
        CurseforgeSearchResult? resourcepacks = await client.SearchResourcepacksAsync("Faithful", "1.19.4");

        // Get Individual Projects
        CurseforgeProject? mod = await client.GetMod("887168");
        CurseforgeProject? modpack = await client.GetModpack("887168");
        CurseforgeProject? resourcepack = await client.GetResourcepack("887168");
        CurseforgeProject? world = await client.GetWorld("887168");

        // Get Project Files
        ModFile[]? modFiles = await client.GetModFiles("887168");
        ModFile[]? modpackFiles = await client.GetModpackFiles("887168");
        ModFile[]? resourcepackFiles = await client.GetResourcepackFiles("887168");
        ModFile[]? worldFiles = await client.GetWorldFiles("887168");

        // Get Specific Project Files
        ModFile? modFile = await client.GetModFile("887168", "250");
        ModFile? modpackFile = await client.GetModpackFile("887168", "250");
        ModFile? resourcepackFile = await client.GetResourcepackFile("887168", "250");
        ModFile? worldFile = await client.GetWorldFile("887168", "250");

        // Downloads the file
        string instancePath = "./minecraft";
        await client.Download(modFile.Value, instancePath, "mods");
        await client.Download(modpackFile.Value, "/path/to/modpacks/");
        await client.Download(resourcepackFile.Value, instancePath, "resourcepack");
        await client.Download(worldFile.Value, instancePath, "saves");
    }
}
```

In these code examples, we first create an instance of the `CurseforgeClient` class and pass the API key as a parameter. Then, we demonstrate various operations such as searching for projects, retrieving project information, getting project files, and downloading files.

Please note that you need to replace `"YOUR_API_KEY"` with your actual CurseForge API key to authenticate with the CurseForge API.

The provided code examples give an overview of how to use the CurseForge.NET library to interact with the CurseForge API. You can further customize and expand the code based on your specific use case and requirements.

Please ensure to handle exceptions and error cases in your code when using the CurseForge.NET library. The provided examples demonstrate basic usage, and you can further customize and utilize the library based on your specific needs. For more information and additional features of the CurseForge.NET library, refer to the library's documentation and API reference.
