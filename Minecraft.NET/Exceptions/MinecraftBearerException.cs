/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Exceptions;

internal class MinecraftBearerException : Exception
{
    public MinecraftBearerException(string message, string token, string responseBody) : base($"{message} XSTS Token: '{token}'\nResponse Body:\n{responseBody}")
    {
    }

    public MinecraftBearerException(string token, string responseBody) : this("Unable to get the Minecraft Bearer authentication token from server", token, responseBody)
    {
    }
}