using System;
using System.Threading.Tasks;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Types;
using Nethereum.Web3;

namespace ComethSDK.Scripts.Services
{
    public static class DelayService
    {
        public static async Task<bool> OnGoingRecovery(string walletAddress, API api, Web3 web3)
        {
            WalletInfos info = await api.GetWalletInfos(walletAddress);

            string proxyDelayAddress = info.proxyDelayAddress;
            var delayContract = web3.Eth.GetContract(Constants.DELAY_ABI, proxyDelayAddress);

            var txNonceFunction = delayContract.GetFunction("txNonce");
            var txNonce = await txNonceFunction.CallAsync<int>();

            var queueNonceFunction = delayContract.GetFunction("queueNonce");
            var queueNonce = await queueNonceFunction.CallAsync<int>();

            return txNonce != queueNonce;
        }

        public static async Task<long> RecoveryCooldown(string walletAddress, API api, Web3 web3)
        {
            WalletInfos info = await api.GetWalletInfos(walletAddress);

            string proxyDelayAddress = info.proxyDelayAddress;
            var delayContract = web3.Eth.GetContract(Constants.DELAY_ABI, proxyDelayAddress);

            var txNonceFunction = delayContract.GetFunction("txNonce");
            var txNonce = await txNonceFunction.CallAsync<int>();

            var txCreatedAtFunction = delayContract.GetFunction("txCreatedAt");
            var txCreatedAt = await txCreatedAtFunction.CallAsync<int>(txNonce);

            var txCooldownFunction = delayContract.GetFunction("txCooldown");
            var txCooldown = await txCooldownFunction.CallAsync<int>();

            long endTime = txCreatedAt + txCooldown;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            long timeRemaining = endTime - currentTime;

            return timeRemaining;
        }

        public static async Task<MetaTransactionData> PrepareCancelRecoveryTx(string walletAddress, API api, Web3 web3)
        {
            WalletInfos info = await api.GetWalletInfos(walletAddress);

            string proxyDelayAddress = info.proxyDelayAddress;
            var delayContract = web3.Eth.GetContract(Constants.DELAY_ABI, proxyDelayAddress);

            var txNonceFunction = delayContract.GetFunction("txNonce");
            var txNonce = await txNonceFunction.CallAsync<int>();

            int newNonce = txNonce + 1;

            var setTxNonceFunction = delayContract.GetFunction("setTxNonce");
            var data = setTxNonceFunction.GetData(newNonce.ToString());

            var tx = new MetaTransactionData
            {
                to = proxyDelayAddress,
                value = "0",
                data = data,
                operation = 0
            };

            return tx;
        }
    }
}