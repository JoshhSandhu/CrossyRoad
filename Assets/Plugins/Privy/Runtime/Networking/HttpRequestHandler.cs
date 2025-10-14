using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Privy
{
    internal class HttpRequestHandler : IHttpRequestHandler
    {

        private string _baseUrl;
        private string _appId;

        private string _clientId;
        private static readonly string ContentType = "application/json";

        public HttpRequestHandler(PrivyConfig privyConfig)
        {
            _appId = privyConfig.AppId;
            _clientId = privyConfig.ClientId;
            _baseUrl = $"{PrivyEnvironment.BASE_URL}/api/v1";
        }

        // Method to send HTTP requests
        public async Task<string> SendRequestAsync(string path, string jsonData, Dictionary<string, string> customHeaders = null)
        {
            var endpoint = $"{_baseUrl}/{path}"; //need to be careful here, to ensure no issues with slashes
            using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
            
                // app id header
                request.SetRequestHeader(Constants.PRIVY_APP_ID_HEADER, _appId);
                PrivyLogger.Debug($"App identifier is {_appId}");

                // client id header
                request.SetRequestHeader(Constants.PRIVY_CLIENT_ID_HEADER, _clientId);
                PrivyLogger.Debug($"Client app identifier is {_clientId}");

                // Need to add native app bundle ID here
                string appIdentifier = Application.identifier;
                if (appIdentifier != null) {
                    PrivyLogger.Debug($"Unity app identifier is {appIdentifier}");
                    request.SetRequestHeader(Constants.PRIVY_NATIVE_APP_IDENTIFIER, appIdentifier);
                } 

                if (customHeaders != null)
                {
                    foreach (var header in customHeaders)
                    {
                        request.SetRequestHeader(header.Key, header.Value);
                    }
                }            

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // Deserialize JSON response into TResponse
                    return request.downloadHandler.text;
                }
                else
                {
                    string errorMessage = $"HTTP request failed: {request.error}";

                    if (request.downloadHandler != null)
                    {
                        string responseBody = request.downloadHandler.text;
                        errorMessage += $" Response Body: {responseBody}";
                    }

                    throw new Exception(errorMessage);
                }
            }
        }
    }
}