using System;
using System.Threading.Tasks;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Interfaces;
using Nethereum.GnosisSafe;
using Nethereum.Web3;

namespace ComethSDK.Scripts.Services
{
	public static class SafeService
	{
		public static async Task<bool> IsSigner(string signerAddress, string walletAddress, string provider, API api)
		{
			try
			{
				await IsDeployed(walletAddress, provider);

				var owner = await IsSafeOwner(walletAddress, signerAddress, provider);

				if (!owner) return false;
			}
			catch
			{
				var predictedWalletAddress = await api.GetWalletAddress(signerAddress);

				if (predictedWalletAddress != walletAddress) return false;
			}

			return true;
		}

		public static string EncodeFunctionData(string functionName, string safeAddress, string provider, params object[] functionInput)
		{
			var web3 = new Web3(provider);
			var contract = web3.Eth.GetContract(Constants.SAFE_ABI, safeAddress);
			var function = contract.GetFunction(functionName);
			var data = function.GetData(functionInput);
			return data;
		}

		private static async Task<bool> IsSafeOwner(string walletAddress, string signerAddress, string provider)
		{
			var web3 = new Web3(provider);
			var service = new GnosisSafeService(web3, walletAddress);
			return await service.IsOwnerQueryAsync(signerAddress);
		}

		//TODO: Implement this
		public static async Task<bool> IsDeployed(string walletAddress, string provider)
		{
			throw new NotImplementedException();
		}

		public static string GetFunctionSelector(IMetaTransactionData metaTransactionData)
		{
			return metaTransactionData.data[..10];
		}

		public static string GetTransactionsTotalValue(IMetaTransactionData[] safeTxData)
		{
			var txValue = 0;
			
			foreach (var safeTx in safeTxData)
			{
				txValue += int.Parse(safeTx.value);
			}
			
			return txValue.ToString();
		}
	}
}