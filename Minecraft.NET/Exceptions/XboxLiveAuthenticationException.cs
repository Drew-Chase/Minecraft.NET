/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Model;

namespace Chase.Minecraft.Exceptions;

internal class XboxLiveAuthenticationException : Exception
{
    public XboxLiveAuthenticationException(string message, MicrosoftToken token, string responseBody) : base($"{message} MSAL Token: '{token}'\nResponse Body:\n{responseBody}")
    {
    }

    public XboxLiveAuthenticationException(MicrosoftToken token, string responseBody) : this("Unable to get the XboxLive authentication token from server", token, responseBody)
    {
    }
}