namespace Privy
{
    static internal class Constants
    {
        // Refresh Defaults
        public const int DEFAULT_EXPIRATION_PADDING_IN_SECONDS = 30;

        // Request Headers
        public const string PRIVY_APP_ID_HEADER = "privy-app-id";

        public const string PRIVY_CLIENT_ID_HEADER = "privy-client-id";

        public const string PRIVY_NATIVE_APP_IDENTIFIER = "x-native-app-identifier";

        //Storage Keys
        public const string INTERNAL_AUTH_SESSION_KEY = "internalAuthSession";
    }
}