using System;
using System.Numerics;
using System.Threading.Tasks;
using ComethSDK.Scripts.Interfaces;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Types;
using ComethSDK.Scripts.Types.MessageTypes;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.Web3;

namespace ComethSDK.Scripts.Services
{
	public static class GasService
	{
		private static readonly decimal REWARD_PERCENTILE = Constants.DEFAULT_REWARD_PERCENTILE;
		private static readonly BigInteger BASE_GAS = Constants.DEFAULT_BASE_GAS;

		public static async Task<BigInteger> GetGasPrice(string provider)
		{
			var web3 = new Web3(provider);
			var ethFeeHistory = await web3.Eth.FeeHistory.SendRequestAsync(
				new HexBigInteger(1),
				new BlockParameter(),
				new[] { REWARD_PERCENTILE });

			var reward = ethFeeHistory.Reward[0][0].Value;
			var baseFee = ethFeeHistory.BaseFeePerGas[0].Value;

			return reward + baseFee + (reward + baseFee) / 10;
		}

		public static async Task VerifyHasEnoughBalance(string from, string to, string value, string data, int nonce,
			string provider)
		{
			var web3 = new Web3(provider);
			var walletBalance = await web3.Eth.GetBalance.SendRequestAsync(from);
			var totalGasCost = await CalculateMaxFees(from, to, value, data, nonce, provider);
			if (walletBalance.Value < totalGasCost)
				throw new Exception("Not enough balance to send this value and pay for gas");
		}

		public static async Task<GasEstimates> EstimateTransactionGas(ISafeTransactionDataPartial safeTxData,
			string from, string provider)
		{
			var safeTxGas = safeTxData.safeTxGas;
			safeTxGas += await CalculateSafeTxGas(safeTxData.data, safeTxData.to, from, provider);

			var gasPrice = safeTxData.gasPrice;
			gasPrice += await GetGasPrice(provider);

			return new GasEstimates { safeTxGas = safeTxGas, baseGas = BASE_GAS, gasPrice = gasPrice };
		}
		

		public static async Task<SafeTx> SetTransactionGas(SafeTx safeTxDataTyped, string from, string provider)
		{
			var gasEstimates = await EstimateTransactionGas(safeTxDataTyped, from, provider);
			safeTxDataTyped.safeTxGas = gasEstimates.safeTxGas;
			safeTxDataTyped.baseGas = gasEstimates.baseGas;
			safeTxDataTyped.gasPrice = gasEstimates.gasPrice;

			return safeTxDataTyped;
		}

		public static async Task<BigInteger> CalculateMaxFees(string from, string to, string value, string data,
			int nonce, string provider)
		{
			var safeTx = Utils.CreateSafeTx(to, value, data, nonce);
			safeTx = await SetTransactionGas(safeTx, from, provider);
			var totalGasCost = (safeTx.safeTxGas + safeTx.baseGas) * safeTx.gasPrice;
			return totalGasCost + BigInteger.Parse(value);
		}

		public static async Task<string> EstimateSafeTxGasWithSimulate( string walletAddress, IMetaTransactionData safeTxData,
		string multisendAddress, string singletonAddress, string simulateTxAcessorAddress, string provider) //TODO: transaction data into a n array to handle multisend
		{
			IMetaTransactionData transaction;
			
			//add multisend later

			transaction = safeTxData;
			transaction.operation = 0;
			
			var isSafeDeployed = await SafeService.IsDeployed(walletAddress, provider); //TODO: this is not implemented
			
			var simulateTxContract = SimulateTxAcessorService.GetContract(simulateTxAcessorAddress, provider);
			var simulateFunction = simulateTxContract.GetFunction("simulate");
			var transactionDataToEstimate = simulateFunction.GetData(transaction);
			
			// if the Safe is not deployed we can use the singleton address to simulate
			var to = isSafeDeployed ? walletAddress : singletonAddress;
			
			var safeFunctionToEstimate =  SafeService.EncodeFunctionData("simulateAndRevert", walletAddress, provider, transactionDataToEstimate);

			try
			{
				var safeTxGas = await CalculateSafeTxGas(safeFunctionToEstimate, to, provider);
				return AddExtraGasForSafety(safeTxGas);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		private static async Task<BigInteger> CalculateSafeTxGas(string data, string to, string from, string provider)
		{
			var web3 = new Web3(provider);
			var ethEstimateGas = new EthEstimateGas(web3.Client);

			var transactionInput = new CallInput
			{
				Data = data,
				To = to,
				From = from
			};
			return await ethEstimateGas.SendRequestAsync(transactionInput);
		}
		
		private static async Task<BigInteger> CalculateSafeTxGas(string data, string to, string provider)
		{
			var web3 = new Web3(provider);
			var ethEstimateGas = new EthEstimateGas(web3.Client);

			var transactionInput = new CallInput
			{
				Data = data,
				To = to
			};
			return await ethEstimateGas.SendRequestAsync(transactionInput);
		}

		private static string AddExtraGasForSafety(BigInteger safeTxGas)
		{
			var safeTxGasInt = (int) safeTxGas;

			return Math.Round(safeTxGasInt * 1.1, 0).ToString();
		}
	}
}