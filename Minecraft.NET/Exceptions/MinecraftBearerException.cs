// LFInteractive LLC. 2021-2024﻿

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