/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Chase.Minecraft.Exceptions;

public class InvalidSystemException : Exception
{
    public InvalidSystemException(Dictionary<OSPlatform, ProcessorArchitecture> SupportedPlatforms) : base($"Unsupported System: {RuntimeInformation.OSDescription}-{RuntimeInformation.ProcessArchitecture}; Supported platforms: {JsonConvert.SerializeObject(SupportedPlatforms)}")
    {
    }
}