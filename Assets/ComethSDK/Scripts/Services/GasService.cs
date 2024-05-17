using System;
using System.Numerics;
using System.Threading.Tasks;
using ComethSDK.Scripts.Enums;
using ComethSDK.Scripts.Interfaces;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Types;
using ComethSDK.Scripts.Types.MessageTypes;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.Web3;
using UnityEngine;

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

		public static async Task VerifyHasEnoughBalance(string from, BigInteger totalGasCost, BigInteger txValue,
			string provider)
		{
			var web3 = new Web3(provider);
			var walletBalance = await web3.Eth.GetBalance.SendRequestAsync(from);
			if (walletBalance.Value < totalGasCost+txValue)
				throw new Exception("Not enough balance to send this value and pay for gas");
		}

		private static BigInteger GetTotalGasCost(GasEstimates gasEstimates)
		{
			var totalGasCost = BigInteger.Add(gasEstimates.safeTxGas, gasEstimates.baseGas);
			return BigInteger.Multiply(totalGasCost, gasEstimates.gasPrice);
		}


		public static async Task<BigInteger> EstimateTransactionGas(IMetaTransactionData[] safeTxDataArray,
			string from, string provider)
		{
			var safeTxGas = BigInteger.Zero;

			foreach (var safeTxData in safeTxDataArray)
				safeTxGas += await CalculateSafeTxGas(safeTxData.data, safeTxData.to, from, provider, safeTxData.value);

			return safeTxGas;
		}

		public static async Task<SafeTx> SetTransactionGas(SafeTx safeTxDataTyped, string from, BigInteger baseGas,
			string provider)
		{
			safeTxDataTyped.safeTxGas =
				await EstimateTransactionGas(new IMetaTransactionData[] { safeTxDataTyped }, from, provider);
			safeTxDataTyped.baseGas = baseGas;
			safeTxDataTyped.gasPrice = await GetGasPrice(provider);

			return safeTxDataTyped;
		}

		public static async Task<(BigInteger,BigInteger,BigInteger,BigInteger)> GetTransactionGasWithSimulate(IMetaTransactionData safeTxDataTyped, string walletAddress,
			string multiSendAddress, string singletonAddress, string simulateTxAccessorAddress, string provider)
		{
			var gasEstimates = await SetTxDataAndGetTxGasWithSimulate(walletAddress, safeTxDataTyped,
				multiSendAddress, singletonAddress, simulateTxAccessorAddress, provider);

			 var safeTxGas = BigInteger.Parse(gasEstimates);
			 var baseGas = BASE_GAS;
			 var gasPrice = await GetGasPrice(provider);
			 var totalGasCost = GetTotalGasCost(new GasEstimates(safeTxGas, baseGas, gasPrice));

			return (safeTxGas, baseGas, gasPrice, totalGasCost);
		}
		
		public static async Task<(BigInteger,BigInteger,BigInteger,BigInteger)> GetTransactionGasWithSimulate(IMetaTransactionData[] safeTxDataTypedArray, string walletAddress,
			string multiSendAddress, string singletonAddress, string simulateTxAccessorAddress, string provider)
		{
			var gasEstimates = await SetTxDataAndGetTxGasWithSimulate(walletAddress, safeTxDataTypedArray,
				multiSendAddress, singletonAddress, simulateTxAccessorAddress, provider);

			var safeTxGas = BigInteger.Parse(gasEstimates);
			var baseGas = BASE_GAS;
			var gasPrice = await GetGasPrice(provider);
			var totalGasCost = GetTotalGasCost(new GasEstimates(safeTxGas, baseGas, gasPrice));

			return (safeTxGas, baseGas, gasPrice, totalGasCost);
		}

		public static async Task<BigInteger> CalculateMaxFees(string from, string to, string value, string data,
			int nonce, BigInteger baseGas, string provider)
		{
			var safeTx = Utils.CreateSafeTx(to, value, data, nonce);
			safeTx = await SetTransactionGas(safeTx, from, baseGas, provider);

			var totalGasCost = (safeTx.safeTxGas + safeTx.baseGas) * safeTx.gasPrice;
			return totalGasCost + BigInteger.Parse(value);
		}

		private static async Task<string> SetTxDataAndGetTxGasWithSimulate(string walletAddress,
			IMetaTransactionData safeTxData, string multiSendAddress, string singletonAddress, 
			string simulateTxAccessorAddress, string provider)
		{
			safeTxData.operation = 0;
			return await EstimateSafeTxGasWithSimulate(safeTxData, walletAddress, singletonAddress, simulateTxAccessorAddress, provider);
		}

		private static async Task<string> SetTxDataAndGetTxGasWithSimulate(string walletAddress,
			IMetaTransactionData[] safeTxData, string multiSendAddress, string singletonAddress,
			string simulateTxAccessorAddress, string provider)
		{
			var multiSendData = MultiSend.EncodeMultiSendArray(safeTxData, provider, multiSendAddress).data;

			IMetaTransactionData transaction = new MetaTransactionData
			{
				to = multiSendAddress,
				value = "0",
				data = multiSendData,
				operation = OperationType.DELEGATE_CALL
			};

			return await EstimateSafeTxGasWithSimulate(transaction, walletAddress, singletonAddress,
				simulateTxAccessorAddress, provider);
		}

		private static async Task<string> EstimateSafeTxGasWithSimulate(IMetaTransactionData transaction, string walletAddress, string singletonAddress,
			string simulateTxAccessorAddress, string provider)
		{
			
			var isSafeDeployed = await SafeService.IsDeployed(walletAddress, provider);

			var simulateTxContract = SimulateTxAcessorService.GetContract(simulateTxAccessorAddress, provider);
			var simulateFunction = simulateTxContract.GetFunction("simulate");

			object[] simulateFunctionInputs =
				{ transaction.to, transaction.value, transaction.data.HexToByteArray(), transaction.operation };
			var transactionDataToEstimate = simulateFunction.GetData(simulateFunctionInputs);

			// if the Safe is not deployed we can use the singleton address to simulate
			var to = isSafeDeployed ? walletAddress : singletonAddress;

			var web3 = new Web3(provider);
			var safeContract = web3.Eth.GetContract(Constants.SAFE_ABI, to);
			var safeFunction = safeContract.GetFunction("simulateAndRevert");
			object[] simulateAndRevertFunctionInputs =
				{ simulateTxAccessorAddress, transactionDataToEstimate.HexToByteArray() };
			var safeFunctionToEstimate = safeFunction.GetData(simulateAndRevertFunctionInputs);

			var transactionToEstimateGas = new CallInput
			{
				Data = safeFunctionToEstimate,
				To = to,
				Value = new HexBigInteger(0)
			};

			try
			{
				var encodedResponse = await safeFunction.CallRawAsync(transactionToEstimateGas);
				Debug.Log("encodedResponse :" + encodedResponse);
			}
			catch (SmartContractCustomErrorRevertException smartContractCustomErrorRevertException)
			{
				var safeTxGas = DecodeSafeTxGas(smartContractCustomErrorRevertException.ExceptionEncodedData);
				return AddExtraGasForSafety(BigInteger.Parse(safeTxGas));
			}

			throw new Exception("Error while estimating gas");
		}

		private static string DecodeSafeTxGas(string encodedSafeTxGas)
		{
			//var encodedSafe = encodedSafeTxGas.Substring(9);
			Debug.Log("encodedSafeTxGasLenght:" + encodedSafeTxGas.Length);
			var hexSubstring = encodedSafeTxGas.Substring(184, 10);
			var hexString = "0x" + hexSubstring;
			var gasNum = Convert.ToUInt64(hexString, 16);
			return gasNum.ToString();
		}

		private static async Task<BigInteger> CalculateSafeTxGas(string data, string to, string from, string provider, string value)
		{
			var web3 = new Web3(provider);
			var ethEstimateGas = new EthEstimateGas(web3.Client);

			var transactionInput = new CallInput
			{
				Data = data,
				To = to,
				From = from,
				Value = value != default ? BigInteger.Parse(value).ToHexBigInteger() : null
			};
			return await ethEstimateGas.SendRequestAsync(transactionInput); // Will return an error if contract is not deployed
		}

		private static string AddExtraGasForSafety(BigInteger safeTxGas)
		{
			var safeTxGasInt = (int)safeTxGas;

			return Math.Round(safeTxGasInt * 1.2, 0).ToString();
		}
	}
}