using System;
using System.Web;

namespace Privy
{
    internal class OAuthResultData
    {
        public string OAuthCode;
        public string OAuthState;

        public static OAuthResultData parseFromUri(Uri uri)
        {
            var queryParams = HttpUtility.ParseQueryString(uri.Query);

            var oauthState = queryParams["privy_oauth_state"];
            var oauthCode = queryParams["privy_oauth_code"];

            return new OAuthResultData
            {
                OAuthCode = oauthCode,
                OAuthState = oauthState
            };
        }
    }
}