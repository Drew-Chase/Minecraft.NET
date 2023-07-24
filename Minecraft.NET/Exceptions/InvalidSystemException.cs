// LFInteractive LLC. 2021-2024﻿
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