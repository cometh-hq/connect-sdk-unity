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
using Nethereum.JsonRpc.Client;
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

		public static async Task VerifyHasEnoughBalance(string from, string to, string value, string data, int nonce,
			string provider)
		{
			var web3 = new Web3(provider);
			var walletBalance = await web3.Eth.GetBalance.SendRequestAsync(from);
			var totalGasCost = await CalculateMaxFees(from, to, value, data, nonce, BASE_GAS, provider);
			if (walletBalance.Value < totalGasCost)
				throw new Exception("Not enough balance to send this value and pay for gas");
		}

		public static async Task VerifyHasEnoughBalance(string walletAddress, GasEstimates gasEstimates, string txValue,
			string provider)
		{
			var web3 = new Web3(provider);
			var walletBalance = await web3.Eth.GetBalance.SendRequestAsync(walletAddress);
			var totalGasCost = GetTotalGasCost(gasEstimates);
			var totalValue = BigInteger.Parse(txValue);
			if (walletBalance.Value < BigInteger.Add(totalGasCost, totalValue))
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
				safeTxGas += await CalculateSafeTxGas(safeTxData.data, safeTxData.to, from, provider);

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

		public static async Task<SafeTx> SetTransactionGasWithSimulate(SafeTx safeTxDataTyped, string walletAddress,
			string multiSendAddress, string singletonAddress, string simulateTxAccessorAddress, string provider)
		{
			var safeTxDataArray = new IMetaTransactionData[] { safeTxDataTyped };
			var gasEstimates = await EstimateSafeTxGasWithSimulate(walletAddress, safeTxDataArray,
				multiSendAddress, singletonAddress, simulateTxAccessorAddress, provider);

			safeTxDataTyped.safeTxGas = BigInteger.Parse(gasEstimates);
			safeTxDataTyped.baseGas = BASE_GAS;
			safeTxDataTyped.gasPrice += await GetGasPrice(provider);

			return safeTxDataTyped;
		}

		public static async Task<BigInteger> CalculateMaxFees(string from, string to, string value, string data,
			int nonce, BigInteger baseGas, string provider)
		{
			var safeTx = Utils.CreateSafeTx(to, value, data, nonce);
			safeTx = await SetTransactionGas(safeTx, from, baseGas, provider);

			var totalGasCost = (safeTx.safeTxGas + safeTx.baseGas) * safeTx.gasPrice;
			return totalGasCost + BigInteger.Parse(value);
		}

		public static async Task<string> EstimateSafeTxGasWithSimulate(string walletAddress,
			IMetaTransactionData[] safeTxData,
			string multiSendAddress, string singletonAddress, string simulateTxAccessorAddress,
			string provider)
		{
			IMetaTransactionData transaction;

			if (safeTxData.Length != 1)
			{
				var multiSendData = MultiSend.EncodeMultiSendArray(safeTxData, provider, multiSendAddress).data;

				transaction = new MetaTransactionData
				{
					to = multiSendAddress,
					value = "0",
					data = multiSendData,
					operation = OperationType.DELEGATE_CALL
				};
			}
			else
			{
				transaction = safeTxData[0];
				transaction.operation = 0;
			}

			//var isSafeDeployed = await SafeService.IsDeployed(walletAddress, provider); //TODO: this is not implemented

			var simulateTxContract = SimulateTxAcessorService.GetContract(simulateTxAccessorAddress, provider);
			var simulateFunction = simulateTxContract.GetFunction("simulate");

			object[] simulateFunctionInputs =
				{ transaction.to, transaction.value, transaction.data.HexToByteArray(), transaction.operation };
			var transactionDataToEstimate = simulateFunction.GetData(simulateFunctionInputs);

			// if the Safe is not deployed we can use the singleton address to simulate
			var to = singletonAddress; //isSafeDeployed ? walletAddress : singletonAddress;

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
			catch (RpcResponseException revertResponseException)
			{
				Debug.Log("Revert Reason:" + revertResponseException.RpcError.Message);
			}
			catch (RpcClientUnknownException e)
			{
				Debug.Log(e);
			}

			return "";
		}

		private static string DecodeSafeTxGas(string encodedSafeTxGas)
		{
			Debug.Log("encodedSafeTxGasLenght:" + encodedSafeTxGas.Length);
			var gasHex = encodedSafeTxGas.Substring(184, 10);
			var gasNum = Convert.ToUInt64(gasHex, 16);
			return gasNum.ToString();
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

		private static string AddExtraGasForSafety(BigInteger safeTxGas)
		{
			var safeTxGasInt = (int)safeTxGas;

			return Math.Round(safeTxGasInt * 1.1, 0).ToString();
		}
	}
}