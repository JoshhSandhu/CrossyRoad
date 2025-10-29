using UnityEngine;

[CreateAssetMenu(fileName = "PrivyConfig", menuName = "Privy/Configuration")]
public class PrivyConfiguration : ScriptableObject
{
    [Header("Privy Settings")]
    [Tooltip("Your Privy App ID from the dashboard")]
    public string appId = "cmgodao4u00c7l50caof1nhau";

    [Tooltip("Your Privy Client ID from the dashboard")]
    public string clientId = "client-WY6RcMvDgyVeD25ssjFivmGBiWdhkddSMon2vEWHm4uTz";

    [Tooltip("Use devnet for testing, mainnet for production")]
    public bool useDevnet = true;

    [Header("authentication Settings")]
    [Tooltip("Require authentication before playing")]
    public bool requireAuthentication = true;

    [Tooltip("auto create embedded wallets for new users")]
    public bool autoCreateWallets = true;

    [Tooltip("Require user password for ceating user wallets")]
    public bool requirePasswordOnCreate = false;

    [Header("UI Settings")]
    [Tooltip("Show wallet connection popup on game start")]
    public bool showAuthOnStart = true;

    [Tooltip("Panel transition duration")]
    public float panelTransitionDuration = 0.5f;

    [Header("Solana Settings")]
    [Tooltip("Enable Solana wallet creation")]
    public bool enableSolana = true;

    [Tooltip("Solana RPC URL *leave empty to use Privy's default")]
    public string solanaRpcUrl = "https://api.devnet.solana.com";

    [Tooltip("Solana network")]
    public SolanaNetwork solanaNetwork = SolanaNetwork.Devnet;

    public enum SolanaNetwork
    {
        Devnet,
        Testnet,
        Mainnet
    }

    //Validation
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(appId) || appId == "cmgodao4u00c7l50caof1nhau")
        {
            Debug.LogWarning("Please set your Privy App ID in the PrivyConfiguration asset!");
        }

        if (string.IsNullOrEmpty(clientId) || clientId == "client-WY6RcMvDgyVeD25ssjFivmGBiWdhkddSMon2vEWHm4uTz")
        {
            Debug.LogWarning("Please set your Privy Client ID in the PrivyConfiguration asset!");
        }
    }
}

