/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Exceptions;

internal class MicrosoftAuthenticationException : Exception
{
    public MicrosoftAuthenticationException(string message, string clientId, string code, string responseBody) : base($"{message} - ClientID: '{clientId}', MSAL Authentication Code from the Browser: '{code}'\nResponse Body:\n{responseBody}")
    {
    }

    public MicrosoftAuthenticationException(string clientId, string code, string responseBody) : this("Unable to get the microsoft authentication token from server", clientId, code, responseBody)
    {
    }
}