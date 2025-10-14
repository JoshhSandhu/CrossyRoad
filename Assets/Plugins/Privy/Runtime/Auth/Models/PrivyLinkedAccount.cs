using Newtonsoft.Json;

namespace Privy
{
    public class PrivyLinkedAccount
    {
        // TODO: convert type to enum
        [JsonProperty("type")] public string Type;

        // TODO: convert below date longs to dates
        [JsonProperty("verified_at")] public long VerifiedAt;

        [JsonProperty("first_verified_at")] public long FirstVerifiedAt;

        [JsonProperty("latest_verified_at")] public long LatestVerifiedAt;
    }

    //Privy Embedded Wallet Account
    public class PrivyEmbeddedWalletAccount : PrivyLinkedAccount
    {
        // Note: This class isn't used for deserialization, so we should remove "JsonProperty" from all the fields
        // in a follow up PR
        [JsonProperty("address")] public string Address { get; set; }

        [JsonProperty("imported")] public bool Imported { get; set; }

        [JsonProperty("wallet_index")] public int WalletIndex { get; set; }

        [JsonProperty("chain_id")] public string ChainId { get; set; }

        [JsonProperty("chain_type")] public string ChainType { get; set; }

        [JsonProperty("wallet_client")] public string WalletClient { get; set; }

        [JsonProperty("wallet_client_type")] public string WalletClientType { get; set; }

        [JsonProperty("connector_type")] public string ConnectorType { get; set; }

        [JsonProperty("public_key")] public string PublicKey { get; set; }

        [JsonProperty("recovery_method")] public string RecoveryMethod { get; set; }

        // TODO: extract this into a helper method of the calling class
        // New method to create an EmbeddedWallet
        internal IEmbeddedWallet CreateEmbeddedWallet(string primaryEmbeddedWalletAddress,
            EmbeddedWalletManager embeddedWalletManager)
        {
            var embeddedWalletDetails = new EmbeddedWalletDetails
            {
                PrimaryWalletAddress = primaryEmbeddedWalletAddress,
                CurrentWalletAddress = this.Address,
                ChainId = this.ChainId,
                RecoveryMethod = this.RecoveryMethod,
                HdWalletIndex = this.WalletIndex
            };

            return new EmbeddedWallet(embeddedWalletDetails, embeddedWalletManager);
        }
    }

    // Note: This class isn't used for deserialization, so we should remove "JsonProperty" from all the fields
    // in a follow up PR
    public class PrivyEmailAccount : PrivyLinkedAccount
    {
        [JsonProperty("address")] public string Address { get; set; }
    }
    
    public class GoogleAccount : PrivyLinkedAccount
    {
        [JsonProperty("Subject")] public string Subject { get; set; }
        
        [JsonProperty("Email")] public string Email { get; set; }
        
        [JsonProperty("Name")] public string Name { get; set; }
    }
    
    public class DiscordAccount : PrivyLinkedAccount
    {
        public string Subject { get; set; }
        
        public string Email { get; set; }
        
        public string UserName { get; set; }
    }

    public class AppleAccount : PrivyLinkedAccount
    {
        public string Subject { get; set; }

        public string Email { get; set; }
    }
}
