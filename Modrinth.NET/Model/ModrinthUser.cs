/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;

namespace Chase.Minecraft.Modrinth.Model;

public sealed class ModrinthUser
{
    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("bio")]
    public string Bio { get; set; }

    [JsonProperty("payout_data")]
    public PayoutData? PayoutData { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("github_id")]
    public int GithubId { get; set; }

    [JsonProperty("avatar_url")]
    public string AvatarUrl { get; set; }

    [JsonProperty("created")]
    public string Created { get; set; }

    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("badges")]
    public int Badges { get; set; }

    [JsonProperty("projects")]
    public ModrinthProject[] Projects { get; set; }
}

public struct PayoutData
{
    [JsonProperty("balance")]
    public double Balance { get; set; }

    [JsonProperty("payout_wallet")]
    public string PayoutWallet { get; set; }

    [JsonProperty("payout_wallet_type")]
    public string PayoutWalletType { get; set; }

    [JsonProperty("payout_address")]
    public string PayoutAddress { get; set; }
}