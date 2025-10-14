using System;
using UnityEngine;

namespace Privy
{
    internal static class LinkedAccountResponseMapper
    {
        public static PrivyLinkedAccount MapToPublic(LinkedAccountResponse response)
        {
            PrivyLogger.Debug($"Mapping response type: {response.GetType().Name}");
        
            if (response is WalletAccountResponse walletResponse)
            {
                if (walletResponse.WalletClientType == "privy" && walletResponse.ConnectorType == "embedded")
                {
                    return new PrivyEmbeddedWalletAccount
                    {
                        Type = walletResponse.Type,
                        Address = walletResponse.Address,
                        VerifiedAt = walletResponse.VerifiedAt,
                        FirstVerifiedAt = walletResponse.FirstVerifiedAt,
                        LatestVerifiedAt = walletResponse.LatestVerifiedAt,
                        Imported = walletResponse.Imported,
                        WalletIndex = walletResponse.WalletIndex,
                        ChainId = walletResponse.ChainId,
                        ChainType = walletResponse.ChainType,
                        WalletClient = walletResponse.WalletClient,
                        WalletClientType = walletResponse.WalletClientType,
                        ConnectorType = walletResponse.ConnectorType,
                        PublicKey = walletResponse.PublicKey,
                        RecoveryMethod = walletResponse.RecoveryMethod
                    };
                }
                else
                {
                    // This is an external privy wallet
                    return null;
                }
            }
            else if (response is EmailAccountResponse emailResponse)
            {
                return new PrivyEmailAccount
                {
                    Type = emailResponse.Type,
                    Address = emailResponse.Address,
                    VerifiedAt = emailResponse.VerifiedAt,
                    FirstVerifiedAt = emailResponse.FirstVerifiedAt,
                    LatestVerifiedAt = emailResponse.LatestVerifiedAt
                };
            }
            else if (response is GoogleOAuthAccountResponse googleOAuthAccountResponse)
            {
                return new GoogleAccount 
                {
                    Subject = googleOAuthAccountResponse.Subject,
                    Email = googleOAuthAccountResponse.Email,
                    Name = googleOAuthAccountResponse.Name,
                    Type = googleOAuthAccountResponse.Type,
                    VerifiedAt = googleOAuthAccountResponse.VerifiedAt,
                    FirstVerifiedAt = googleOAuthAccountResponse.FirstVerifiedAt,
                    LatestVerifiedAt = googleOAuthAccountResponse.LatestVerifiedAt
                };
            }
            else if (response is DiscordOAuthAccountResponse discordOAuthAccountResponse)
            {
                return new DiscordAccount
                {
                    Subject = discordOAuthAccountResponse.Subject,
                    Email = discordOAuthAccountResponse.Email,
                    UserName = discordOAuthAccountResponse.UserName,
                    Type = discordOAuthAccountResponse.Type,
                    VerifiedAt = discordOAuthAccountResponse.VerifiedAt,
                    FirstVerifiedAt = discordOAuthAccountResponse.FirstVerifiedAt,
                    LatestVerifiedAt = discordOAuthAccountResponse.LatestVerifiedAt
                };
            }
            else if (response is AppleOAuthAccountResponse appleOAuthAccountResponse)
            {
                return new AppleAccount
                {
                    Subject = appleOAuthAccountResponse.Subject,
                    Email = appleOAuthAccountResponse.Email,
                    Type = appleOAuthAccountResponse.Type,
                    VerifiedAt = appleOAuthAccountResponse.VerifiedAt,
                    FirstVerifiedAt = appleOAuthAccountResponse.FirstVerifiedAt,
                    LatestVerifiedAt = appleOAuthAccountResponse.LatestVerifiedAt
                };
            }
            else
            {
                PrivyLogger.Debug("Mapping to base type PrivyLinkedAccount");
                return null;
            }
        }
    }
}