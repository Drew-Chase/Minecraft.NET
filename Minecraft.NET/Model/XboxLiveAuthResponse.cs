// LFInteractive LLC. 2021-2024﻿
using Newtonsoft.Json;
using System.Text.Json;

namespace Chase.Minecraft.Model;

public struct XboxLiveAuthResponse
{
    [JsonProperty("NotAfter")]
    public DateTime NotAfter;

    [JsonProperty("IssueInstant")]
    public DateTime IssueInstant { get; set; }

    [JsonProperty("Token")]
    public string Token { get; set; }

    [JsonProperty("DisplayClaims")]
    public XboxLiveDisplayClaims DisplayClaims { get; set; }
}

public struct XboxLiveDisplayClaims
{
    [JsonProperty("xui")]
    public List<XboxLiveXUIClaim> XUI { get; set; }
}

public struct XboxLiveXUIClaim
{
    [JsonProperty("uhs")]
    public string UHS { get; set; }
}