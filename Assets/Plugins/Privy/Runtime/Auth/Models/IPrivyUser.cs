using System.Threading.Tasks;
using System.Collections.Generic;

namespace Privy
{
    /// <summary>
    /// Represents a Privy user with properties and methods for managing the user's identity and embedded wallets.
    /// </summary>
    public interface IPrivyUser
    {
        /// <summary>
        /// Gets the user's unique identifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the list of the user's linked accounts.
        /// </summary>
        PrivyLinkedAccount[] LinkedAccounts { get; }

        /// <summary>
        /// Gets the list of the user's embedded wallets.
        /// </summary>
        IEmbeddedWallet[] EmbeddedWallets { get; }

        /// <summary>
        /// Gets the user's custom metadata key-value mapping.
        /// </summary>
        Dictionary<string, string> CustomMetadata { get; }

        /// <summary>
        /// Creates a new embedded wallet for the user.
        /// </summary>
        /// <param name="allowAditional">Whether to allow the creation of additional wallets derived from the primary HD wallet</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created embedded wallet.</returns>
        /// <exception cref="PrivyException.AuthenticationException">
        /// Thrown if there is an issue with authentication, such as a failure to refresh the access token.
        /// </exception>
        /// <exception cref="PrivyException.EmbeddedWalletException">
        /// Thrown if the wallet creation fails or the wallet cannot be added to the user's account.
        /// </exception>
        Task<IEmbeddedWallet> CreateWallet(bool allowAdditional = false);
    }
}