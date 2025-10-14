using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Privy
{
    internal static class PrivyLinkedAccountExtensions
    {
        //These methods allow us to call methods on linkedAccounts, to get data specific to the types of account, such as EmbeddedWalletAccounts

        // Extension method to get the primary embedded wallet account (where walletIndex == 0)
        public static PrivyEmbeddedWalletAccount PrimaryEmbeddedWalletAccountOrNull(this PrivyLinkedAccount[] accounts)
        {
            return accounts
                .OfType<PrivyEmbeddedWalletAccount>()
                .FirstOrDefault(account => account.WalletIndex == 0);
        }

        // Extension method to get all embedded wallet accounts
        public static PrivyEmbeddedWalletAccount[] EmbeddedWalletAccounts(this PrivyLinkedAccount[] accounts)
        {
            return accounts
                .OfType<PrivyEmbeddedWalletAccount>()
                .ToArray();
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OAuthProvider
    {
        [EnumMember(Value = "google")]
        Google,
        
        [EnumMember(Value = "apple")]
        Apple,

        [EnumMember(Value = "discord")]
        Discord
    }

    public class PrivyUser : IPrivyUser
    {
        public string Id
        {
            get
            {
                if (_authDelegator.CurrentAuthState != AuthState.Authenticated)
                {
                    return "";
                }
                else
                {
                    return _authDelegator.GetAuthSession().User.Id;
                }
            }
        }


        public PrivyLinkedAccount[] LinkedAccounts
        {
            get
            {
                if (_authDelegator.CurrentAuthState != AuthState.Authenticated)
                {
                    return Array.Empty<PrivyLinkedAccount>();
                }
                else
                {
                    return _authDelegator.GetAuthSession().User.LinkedAccounts;
                }
            }
        }

        //This getter parses EmbeddedWalletAccounts from the Linked Accounts, and then converts them to type EmbeddedWallet, and returns an array of them
        public IEmbeddedWallet[] EmbeddedWallets
        {
            get
            {
                PrivyEmbeddedWalletAccount primaryEmbeddedWalletAccount =
                    LinkedAccounts.PrimaryEmbeddedWalletAccountOrNull();
                //We fetch this so we can attach the address of this to each embedded wallet

                // If no primary embedded wallet account exists, return an empty array
                if (primaryEmbeddedWalletAccount == null)
                {
                    return Array.Empty<IEmbeddedWallet>();
                }

                // Otherwise, create the array of embedded wallets using the primary wallet's address
                var embeddedWallets = LinkedAccounts.EmbeddedWalletAccounts()
                    .Select(account =>
                        account.CreateEmbeddedWallet(primaryEmbeddedWalletAccount.Address, this._embeddedWalletManager))
                    .ToArray();

                return embeddedWallets;
            }
        }

        public Dictionary<string, string> CustomMetadata
        {
            get
            {
                if (_authDelegator.CurrentAuthState != AuthState.Authenticated)
                {
                    return new Dictionary<string, string>();
                }
                else
                {
                    return _authDelegator.GetAuthSession().User.CustomMetadata;
                }
            }
        }

        private AuthDelegator _authDelegator;

        private EmbeddedWalletManager _embeddedWalletManager;
        private const int MAX_WALLETS_ALLOWED = 10;


        //Constructor is internal, but the object is public facing
        //TODO: Create Interface class for this
        internal PrivyUser(AuthDelegator authDelegator, EmbeddedWalletManager embeddedWalletManager)
        {
            this._authDelegator = authDelegator;
            this._embeddedWalletManager = embeddedWalletManager;
        }

        public async Task<IEmbeddedWallet> CreateWallet(bool allowAdditional = false)
        {
            string address = await CreatePrimaryWalletOrAdditional(allowAdditional); //This can throw an error

            await _authDelegator.RefreshSession(true); //Forces refresh even if token is valid

            var embeddedWallet =
                EmbeddedWallets.FirstOrDefault(wallet =>
                    wallet.Address == address); //Get the first wallet with matching address, else null

            //We could technically return null here, and it would be more performant if we did
            //But to maintain consistency - I think we go with the approach of the dev catching errors, as oppose to parsing nulls
            //If we returned a null here, the docs also become inconsistent, where in some places they'd be checking for nulls, and other places they'd be catching errors
            if (embeddedWallet == null)
            {
                throw new PrivyException.EmbeddedWalletException(
                    "Wallet Create Failed: Wallet was not added to account.", EmbeddedWalletError.CreateFailed);
            }


            //This mimics a background process while being non-blocking
            //Mimics await functionality without waiting for result, by using a callback to process the result
            Task connectTask = _embeddedWalletManager.AttemptConnectingToWallet(_authDelegator.GetAuthSession());
            connectTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // Handle the error silently or log it without interrupting the main flow
                    PrivyLogger.Error("Could not connect wallet", task.Exception);
                }
            });

            return embeddedWallet;
        }

        private async Task<string> CreatePrimaryWalletOrAdditional(bool allowAdditional)
        {
            string
                token = await _authDelegator
                    .GetAccessToken(); //This could throw an error if the access token needs a refresh, and the refresh fails

            var primaryEmbeddedWalletAccount = LinkedAccounts.PrimaryEmbeddedWalletAccountOrNull();
            var currentWalletCount = EmbeddedWallets.Length;

            if (primaryEmbeddedWalletAccount == null)
            {
                string address = await _embeddedWalletManager.CreateWallet(token); //This can throw an error
                return address;
            }
            else if (!allowAdditional)
            {
                throw new PrivyException.EmbeddedWalletException(
                    "Wallet Create Failed: Wallet already exists. To create an additional wallet, set allowAdditional to true.",
                    EmbeddedWalletError.CreateFailed);
            }
            else if (currentWalletCount >= MAX_WALLETS_ALLOWED)
            {
                throw new PrivyException.EmbeddedWalletException(
                    $"Wallet Create Failed: Maximum number of wallets ({MAX_WALLETS_ALLOWED}) reached.",
                    EmbeddedWalletError.CreateAdditionalFailed);
            }
            else
            {
                string primaryWalletAddress = primaryEmbeddedWalletAccount.Address;
                int hdWalletIndex =
                    currentWalletCount; // HD Index is always sequential, starting on 0 for the primary wallet.
                string address =
                    await _embeddedWalletManager.CreateAdditionalWallet(token, primaryWalletAddress,
                        hdWalletIndex); //This can throw an error
                return address;
            }
        }
    }

    public class PrivyAuthSession
    {
        public PrivyUser User;
    }
}