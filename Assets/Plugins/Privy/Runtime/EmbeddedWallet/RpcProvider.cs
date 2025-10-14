using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Privy
{
    internal class RpcProvider : IRpcProvider
    {
        public string PrimaryWalletAddress { get; }
        public int HdWalletIndex { get; }

        private EmbeddedWalletManager _embeddedWalletManager;

        private static readonly HashSet<string> _allowedMethods = new HashSet<string>
        {
            "eth_sign",
            "personal_sign",
            "eth_populateTransactionRequest",
            "eth_signTypedData_v4",
            "eth_signTransaction",
            "eth_sendTransaction"
        };

        public RpcProvider(string primaryWalletAddress, int hdWalletIndex, EmbeddedWalletManager embeddedWalletManager)
        {
            this.PrimaryWalletAddress = primaryWalletAddress;
            this.HdWalletIndex = hdWalletIndex;
            _embeddedWalletManager = embeddedWalletManager;
        }

        public async Task<RpcResponse> Request(RpcRequest request)
        {
            if (_allowedMethods.Contains(request.Method)) {
                return await _embeddedWalletManager.Request(PrimaryWalletAddress, HdWalletIndex, request);
            } else {
                return await HandleJsonRpc(request);
            }
        }

        private async Task<RpcResponse> HandleJsonRpc(RpcRequest request) {
            PrivyLogger.Debug("Unsupported rpc request type");
            return null;
        }
    }
}