using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Interfaces;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Types;
using Nethereum.GnosisSafe;
using Nethereum.Web3;

namespace ComethSDK.Scripts.Services
{
	public static class SafeService
	{
		public static async Task<bool> IsSigner(string signerAddress, string walletAddress, string rpcUrl, API api)
		{
			try
			{
				var owner = await IsSafeOwner(walletAddress, signerAddress, rpcUrl);

				if (!owner) return false;
			}
			catch
			{
				var predictedWalletAddress = await api.GetWalletAddress(signerAddress);

				if (predictedWalletAddress != walletAddress) return false;
			}

			return true;
		}

		public static string EncodeFunctionData(string functionName, string safeAddress, string rpcUrl,
			params object[] functionInput)
		{
			var web3 = new Web3(rpcUrl);
			var contract = web3.Eth.GetContract(Constants.SAFE_ABI, safeAddress);
			var function = contract.GetFunction(functionName);
			var data = function.GetData(functionInput);
			return data;
		}

		public static async Task<MetaTransactionData> PrepareAddOwnerTx(string walletAddress, string newOwner,
			string rpcUrl)
		{
			var web3 = new Web3(rpcUrl);
			var contract = web3.Eth.GetContract(Constants.SAFE_ABI, walletAddress);
			var addOwnerWithThresholdFunction = contract.GetFunction("addOwnerWithThreshold");
			var data = addOwnerWithThresholdFunction.GetData(newOwner, 1);

			var tx = new MetaTransactionData
			{
				to = walletAddress,
				value = "0",
				data = data,
				operation = 0
			};

			return tx;
		}

		public static async Task<MetaTransactionData> PrepareRemoveOwnerTx(string walletAddress, string ownerAddress,
			string rpcUrl)
		{
			var prevOwner = await GetSafePreviousOwner(walletAddress, ownerAddress, rpcUrl);

			var web3 = new Web3(rpcUrl);
			var contract = web3.Eth.GetContract(Constants.SAFE_ABI, walletAddress);
			var removeOwnerFunction = contract.GetFunction("removeOwner");
			var data = removeOwnerFunction.GetData(prevOwner, ownerAddress, 1);

			var tx = new MetaTransactionData
			{
				to = walletAddress,
				value = "0",
				data = data,
				operation = 0
			};

			return tx;
		}

		public static async Task<string> GetSafePreviousOwner(string walletAddress, string owner, string rpcUrl)
		{
			var ownerList = await GetOwners(walletAddress, rpcUrl);

			var findIndex = ownerList.FindIndex(ownerToFind => ownerToFind == owner);
			if (findIndex == -1) throw new Exception("Address is not an owner of the wallet");

			var prevOwner = Constants.SAFE_SENTINEL_OWNERS;

			if (findIndex != 0) prevOwner = ownerList[findIndex - 1];

			return prevOwner;
		}

		public static async Task<List<string>> GetOwners(string walletAddress, string rpcUrl)
		{
			if (!await IsDeployed(walletAddress, rpcUrl)) throw new Exception("Wallet is not deployed");

			var web3 = new Web3(rpcUrl);
			var contract = web3.Eth.GetContract(Constants.SAFE_ABI, walletAddress);
			var getOwnersFunction = contract.GetFunction("getOwners");
			var owners = await getOwnersFunction.CallAsync<List<string>>();

			return owners;
		}

		private static async Task<bool> IsSafeOwner(string walletAddress, string signerAddress, string rpcUrl)
		{
			if (!await IsDeployed(walletAddress, rpcUrl)) throw new Exception("Wallet is not deployed");

			var web3 = new Web3(rpcUrl);
			var service = new GnosisSafeService(web3, walletAddress);

			return await service.IsOwnerQueryAsync(signerAddress);
		}

		public static async Task<bool> IsDeployed(string walletAddress, string rpcUrl)
		{
			var web3 = new Web3(rpcUrl);
			var result = await web3.Eth.GetCode.SendRequestAsync(walletAddress);

			if (result == "0x") return false;

			return true;
		}

		public static string GetFunctionSelector(IMetaTransactionData metaTransactionData)
		{
			return metaTransactionData.data.Length < 10 ? metaTransactionData.data : metaTransactionData.data[..10];
		}

		public static string GetTransactionsTotalValue(IMetaTransactionData[] safeTxData)
		{
			var txValue = 0;

			foreach (var safeTx in safeTxData) txValue += int.Parse(safeTx.value);

			return txValue.ToString();
		}
	}
}