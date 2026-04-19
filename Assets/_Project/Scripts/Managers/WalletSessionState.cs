using UnityEngine;

/// <summary>
/// Single source of truth for wallet connection state across the app.
/// All scripts must read from and write to this class.
/// </summary>
public static class WalletSessionState
{
    public static bool IsSeekerConnected { get; set; } = false;
    public static bool IsPrivyConnected { get; set; } = false;

    private const string KEY_LOGIN_METHOD = "LoginMethod";
    public const string LOGIN_SEEKER = "seeker";
    public const string LOGIN_PRIVY = "privy";
    public const string LOGIN_BOTH = "both";
    public const string LOGIN_NONE = "none";

    public static void SetLoginMethod(string method)
    {
        PlayerPrefs.SetString(KEY_LOGIN_METHOD, method);
        PlayerPrefs.Save();
    }

    public static string GetLoginMethod()
    {
        return PlayerPrefs.GetString(KEY_LOGIN_METHOD, LOGIN_NONE);
    }

    public static void ClearLoginMethod()
    {
        PlayerPrefs.DeleteKey(KEY_LOGIN_METHOD);
        PlayerPrefs.Save();
    }
}
