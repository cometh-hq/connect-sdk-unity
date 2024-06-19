using System.IO;
using System.Threading.Tasks;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Types;
using Nethereum.Web3;
using UnityEngine;

namespace ComethSDK.Scripts.Services
{
    public static class DelayService
    {
        public static async Task<bool> OnGoingRecovery(string walletAddress, API api, Web3 web3)
        {
            WalletInfos info = await api.GetWalletInfos(walletAddress);

            string abiPath = Path.Combine(Application.dataPath, "ComethSDK/ABI", "delay.json");
            string abi = File.ReadAllText(abiPath);

            string proxyDelayAddress = info.proxyDelayAddress;
            var delayContract = web3.Eth.GetContract(abi, proxyDelayAddress);

            var txNonceFunction = delayContract.GetFunction("txNonce");
            var txNonce = await txNonceFunction.CallAsync<int>();

            var queueNonceFunction = delayContract.GetFunction("queueNonce");
            var queueNonce = await queueNonceFunction.CallAsync<int>();

            return txNonce != queueNonce;
        }

        public static async Task<MetaTransactionData> PrepareCancelRecoveryTx(string walletAddress, API api, Web3 web3)
        {
            WalletInfos info = await api.GetWalletInfos(walletAddress);

            string abiPath = Path.Combine(Application.dataPath, "ComethSDK/ABI", "delay.json");
            string abi = File.ReadAllText(abiPath);

            string proxyDelayAddress = info.proxyDelayAddress;
            var delayContract = web3.Eth.GetContract(abi, proxyDelayAddress);

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