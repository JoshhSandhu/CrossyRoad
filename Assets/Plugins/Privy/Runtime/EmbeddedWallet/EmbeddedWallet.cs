using System;

namespace Privy
{
    internal class EmbeddedWallet : IEmbeddedWallet
    {
        public string Address { get; }
        public string ChainId { get; }
        public string RecoveryMethod { get; }
        public int HdWalletIndex { get; }

        public IRpcProvider RpcProvider { get; }  // Use IRpcProvider as the type

        public EmbeddedWallet(EmbeddedWalletDetails embeddedWalletDetails, EmbeddedWalletManager embeddedWalletManager)
        {
            Address = embeddedWalletDetails.CurrentWalletAddress;
            ChainId = embeddedWalletDetails.ChainId;
            RecoveryMethod = embeddedWalletDetails.RecoveryMethod;
            HdWalletIndex = embeddedWalletDetails.HdWalletIndex;

            RpcProvider = new RpcProvider(embeddedWalletDetails.PrimaryWalletAddress, embeddedWalletDetails.HdWalletIndex, embeddedWalletManager);  // Assign a RpcProvider instance
        }
    }
}