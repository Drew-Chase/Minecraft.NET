// LFInteractive LLC. 2021-2024﻿
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