# Fabric.NET API Documentation

## Introduction

Fabric.NET is a library designed to handle the downloading and installation of Fabric loaders for Minecraft instances. It provides a simple and convenient way to manage Fabric loader versions and associate them with specific Minecraft instances.

## Prerequisites

Before using Fabric.NET, ensure that you have:

- .NET Core or .NET 5.0 installed.
- Minecraft instances set up using the `InstanceManager` from Chase.Minecraft.Instances.

## Installation

Fabric.NET can be installed using NuGet. Open your project in Visual Studio or any other compatible IDE and run the following command in the Package Manager Console:

```bash
Install-Package Fabric.NET
```

## Usage

### Getting Fabric Loader Versions

To get an array of all available Fabric loader versions, use the `FabricLoader.GetLoaderVersions` method:

```csharp
// Gets the latest loader versions.
string[] loader_versions = await FabricLoader.GetLoaderVersions();
```

### Installing Fabric Loader to a Minecraft Instance

To download and install a specific Fabric loader version to a Minecraft instance, use the `FabricLoader.Install` method:

```csharp
// Gets the instance from the instance manager.
InstanceManager manager = new InstanceManager("./instances");
InstanceModel instance = manager.GetFirstInstancesByName("Test");

// Downloads and installs the specified Fabric loader version to the specified instance.
await FabricLoader.Install(loader_versions.First(), instance);
```
