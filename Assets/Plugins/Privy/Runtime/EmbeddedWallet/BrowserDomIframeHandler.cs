// This class handles all logic related to managing the iframe, including injection and message communication.
// 
// Why we're using an iframe:
// In WebGL builds, there is no native support for WebView functionality. To simulate WebView behavior,
// webview packages typically inject an iframe under the hood. However, using these iframes presents certain limitations.
// 
// Key reasons for our custom iframe implementation:
// 1. Control Limitations: The webview package used in this SDK injects an iframe, but it does not support 
//    the use of `postMessage` for communication. Our only alternative with these packages is to use `eval`, 
//    which is blocked when running cross-origin, limiting functionality.
// 
// 2. Origin Security: When dealing with iframes, especially in cross-origin contexts, it’s critical to perform 
//    security checks, such as verifying the origin of messages before processing them. This is necessary to 
//    prevent potential security risks like accepting messages from untrusted sources.
// 
// 3. Message Control: With the custom iframe implementation, we have full control over when and how messages 
//    are sent and received, ensuring that communication between the iframe and Unity is secure and reliable.
//    For example, the endpoint sends us JSON objects, which we need to stringify to send to unity
// 
// Due to these limitations and the need for greater control, we opted for a custom implementation. 
// By directly injecting the iframe and managing all message communication ourselves, we gain the flexibility 
// and security required for this SDK.

#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using UnityEngine;

namespace Privy
{
    internal class BrowserDomIframeHandler : IWebViewHandler
    {
        private readonly BrowserDomIframeObject _browserDomIframeObject;
        private readonly WebViewManager _webViewManager; // Reference to WebViewManager

        internal BrowserDomIframeHandler(WebViewManager webViewManager)
        {
            _webViewManager = webViewManager; // Assign WebViewManager reference

            _browserDomIframeObject = GameObject.FindObjectOfType<BrowserDomIframeObject>();

            _browserDomIframeObject.Initialize(webViewManager);
        }

        public void LoadUrl(string url)
        {
            InjectIframe(url);
        }

        public void SendMessage(string message)
        {
            // Properly escape the message to be a valid JavaScript string literal
            string jsMessage = System.Web.HttpUtility.JavaScriptStringEncode(message);

            string jsCode = $@"
            var iframe = document.getElementById('myIframe');

            // Wrap the message in quotes for JavaScript
            var message = '{jsMessage}';

            if (message.includes('iframe')) {{
                //The ready message should ensure that the iframe is loaded
                iframe.onload = function() {{
                    if (iframe && iframe.contentWindow) {{
                        iframe.contentWindow.postMessage(message, '{PrivyEnvironment.BASE_URL}');
                    }}
                }};
            }} else {{
                if (iframe && iframe.contentWindow) {{
                    iframe.contentWindow.postMessage(message, '{PrivyEnvironment.BASE_URL}');
                }}                
            }}
        ";
            Application.ExternalEval(jsCode);
        }

        private void InjectIframe(string url)
        {
            //This function is executing some javascript into the WebGL build
            //This javascript essentially creating an iframe in the DOM, and making it headless by setting the display value to none
            //The source of the iframe, is the Privy embedded wallet url
            //We also add an event listener on to the page, to listen to events coming from the iframe
            string jsCode = $@"
            console.log('Privy: Creating iframe with URL:', '{url}');
            var iframe = document.createElement('iframe');
            iframe.id = 'myIframe';
            iframe.style.position = 'absolute';
            iframe.style.display = 'none';
            
            // Add error handlers for debugging
            iframe.onerror = function(error) {{
                console.error('Privy iframe error:', error);
            }};
            
            iframe.onload = function() {{
                console.log('Privy iframe loaded successfully');
                
                // Try to access iframe content (will fail due to CORS for cross-origin, but that's OK)
                setTimeout(function() {{
                    try {{
                        var iframeDoc = iframe.contentDocument || iframe.contentWindow.document;
                        var iframeBody = iframeDoc.body;
                        console.log('Privy: Iframe body content:', iframeBody ? iframeBody.innerHTML.substring(0, 100) : 'null');
                        console.log('Privy: Iframe document readyState:', iframeDoc.readyState);
                    }} catch(e) {{
                        console.log('Privy: Cannot access iframe content (CORS - this is normal for cross-origin):', e.message);
                    }}
                    
                    // Try to check if iframe has any errors
                    try {{
                        var iframeWin = iframe.contentWindow;
                        // Override console methods in iframe to catch errors
                        if (iframeWin && iframeWin.console) {{
                            var originalError = iframeWin.console.error;
                            iframeWin.console.error = function() {{
                                console.error('Privy iframe error:', Array.from(arguments));
                                originalError.apply(iframeWin.console, arguments);
                            }};
                        }}
                    }} catch(e) {{
                        // CORS prevents this, but that's OK
                        console.log('Privy: Cannot access iframe console (CORS):', e.message);
                    }}
                }}, 2000); // Wait 2 seconds for iframe to fully load
            }};
            
            iframe.onabort = function() {{
                console.error('Privy iframe loading aborted');
            }};
            
            // Set src after appending to body (helps with some browser security policies)
            document.body.appendChild(iframe);
            console.log('Privy: Setting iframe src to:', '{url}');
            iframe.src = '{url}';

            // Helper function to get unityInstance (waits for it if not ready)
            function getUnityInstance() {{
                if (typeof window.unityInstance !== 'undefined' && window.unityInstance !== null) {{
                    return window.unityInstance;
                }}
                if (typeof unityInstance !== 'undefined' && unityInstance !== null) {{
                    return unityInstance;
                }}
                return null;
            }}
            
            // Setup message listener - will be called when iframe sends messages
            window.addEventListener('message', function(event) {{
                // Check that the message is coming from the correct origin
                if (event.origin === new URL(iframe.src).origin) {{
                    // Get unityInstance (with retry logic if not ready yet)
                    var unity = getUnityInstance();
                    
                    if (!unity) {{
                        // Unity might not be loaded yet - wait a bit and try again
                        console.warn('Privy: unityInstance not found yet. Will retry when Unity loads. Set window.unityInstance in your Unity template!');
                        // Try to wait for Unity to be ready
                        var retryCount = 0;
                        var maxRetries = 10;
                        var checkUnity = setInterval(function() {{
                            unity = getUnityInstance();
                            if (unity || retryCount >= maxRetries) {{
                                clearInterval(checkUnity);
                                if (!unity) {{
                                    console.error('Privy: unityInstance still not found after waiting. Make sure your Unity WebGL template sets window.unityInstance = unityInstance;');
                                    return;
                                }}
                                // Unity is now ready, process the message
                                if(event.data === 'ready') {{
                                    unity.SendMessage('{BrowserDomIframeObject.SingletonGameObjectName}', 'OnWebViewReady', '');
                                }} else {{
                                    let data = JSON.stringify(event.data);
                                    unity.SendMessage('{BrowserDomIframeObject.SingletonGameObjectName}', 'OnMessageReceived', data);
                                }}
                            }}
                            retryCount++;
                        }}, 500); // Check every 500ms
                        return;
                    }}
                    
                    if(event.data === 'ready') {{
                        unity.SendMessage('{BrowserDomIframeObject.SingletonGameObjectName}', 'OnWebViewReady', '');
                    }} else {{
                        let data = JSON.stringify(event.data);
                        unity.SendMessage('{BrowserDomIframeObject.SingletonGameObjectName}', 'OnMessageReceived', data);
                    }}
                }} else {{
                    console.warn('Message received from unknown origin:', event.origin);
                }}
            }});
        ";

            Application.ExternalEval(jsCode);

            // Trigger PingReadyUntilSuccessful after injecting the iframe
            _webViewManager.PingReadyUntilSuccessful();        
        }
    }
}
#endif