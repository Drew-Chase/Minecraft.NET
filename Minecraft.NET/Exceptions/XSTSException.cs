// LFInteractive LLC. 2021-2024﻿
using Chase.Minecraft.Model;

namespace Chase.Minecraft.Exceptions;

internal class XSTSException : Exception
{
    public XSTSException(string message, XboxLiveAuthResponse token, string responseBody) : base($"{message} XboxLive Token: '{token}'\nResponse Body:\n{responseBody}")
    {
    }

    public XSTSException(XboxLiveAuthResponse token, string responseBody) : this("Unable to get the XSTS authentication token from server", token, responseBody)
    {
    }
}