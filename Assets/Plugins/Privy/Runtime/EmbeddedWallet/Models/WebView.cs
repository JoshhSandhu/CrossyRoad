using Newtonsoft.Json;

namespace Privy
{
    public class IframeRequest<T>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }
    
    public class ReadyRequestData
    {

    }

    public class CreateWalletRequestData
    {
        [JsonProperty("accessToken")]
        public string AccessToken;
    }

    public class CreateAdditionalWalletRequestData
    {
        [JsonProperty("accessToken")]
        public string AccessToken;

        [JsonProperty("primaryWalletAddress")]
        public string PrimaryWalletAddress;

        [JsonProperty("hdWalletIndex")]
        public int HdWalletIndex;
    }

    public class ConnectWalletRequestData
    {
        [JsonProperty("accessToken")]
        public string AccessToken;

        [JsonProperty("address")]
        public string Address;
    }

    public class RecoverWalletRequestData
    {
        [JsonProperty("accessToken")]
        public string AccessToken;

        [JsonProperty("address")]
        public string Address;
    }

    public class RpcRequestData
    {

        [JsonProperty("accessToken")]
        public string AccessToken { get; set; } 

        [JsonProperty("address")]
        public string Address { get; set; } 

        [JsonProperty("hdWalletIndex")]
        public int HdWalletIndex { get; set; }

        [JsonProperty("request")]
        public RequestDetails Request { get; set; }

        public class RequestDetails
        {
            [JsonProperty("method")]
            public string Method { get; set; } 
            
            [JsonProperty("params")]
            public string[] Params { get; set; }
        }
    }

    //Responses

    //Base Class, used to parse event and id
    public class IframeResponse
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }
    }

    public class IframeResponseSuccess<T> : IframeResponse
    {

        [JsonProperty("data")]
        public T Data { get; set; }
    }

    public class ErrorDetails
    {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("message")]

        public string Message { get; set; }
    }


    public class IframeResponseError : IframeResponse
    {

        [JsonProperty("error")]
        public ErrorDetails Error { get; set; }

    }

    public class ReadyResponseData
    {
        // Add specific properties for iframe ready data
    }


    public class CreateWalletResponseData
    {

        [JsonProperty("address")]
        public string Address { get; set; }
    }

    public class CreateAdditionalWalletResponseData
    {

        [JsonProperty("address")]
        public string Address { get; set; }
    }

    public class ConnectWalletResponseData
    {
        [JsonProperty("address")]
        public string Address { get; set; }
    }

    public class RecoverWalletResponseData
    {

        [JsonProperty("address")]
        public string Address;
    }

    public class RpcResponseData
    {

        [JsonProperty("address")]
        public string Address { get; set; } 

        [JsonProperty("response")]
        public ResponseDetails Response { get; set; }

        public class ResponseDetails
        {

            [JsonProperty("method")]
            public string Method { get; set; }

            [JsonProperty("data")]
            public string Data { get; set; }
        }
    }
}