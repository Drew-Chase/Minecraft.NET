// LFInteractive LLC. 2021-2024﻿
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