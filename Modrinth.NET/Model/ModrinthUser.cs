// LFInteractive LLC. 2021-2024﻿
using Newtonsoft.Json;

namespace Chase.Minecraft.Modrinth.Model;

public struct ModrinthUser
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