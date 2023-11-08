using System;
using System.Numerics;
using System.Threading.Tasks;
using ComethSDK.Scripts.Interfaces;
using ComethSDK.Scripts.Tools;
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

		public static async Task<BigInteger> GetGasPrice(IWeb3 web3)
		{
			var ethFeeHistory = await web3.Eth.FeeHistory.SendRequestAsync(
				new HexBigInteger(1),
				new BlockParameter(),
				new[] { REWARD_PERCENTILE });

			var reward = ethFeeHistory.Reward[0][0].Value;
			var baseFee = ethFeeHistory.BaseFeePerGas[0].Value;

			return reward + baseFee + (reward + baseFee) / 10;
		}

		public static async Task VerifyHasEnoughBalance(string walletAddress, BigInteger safeTxGas, BigInteger gasPrice, BigInteger baseGas, string txValue,
			IWeb3 web3)
		{
			var walletBalance = await web3.Eth.GetBalance.SendRequestAsync(walletAddress);
			var totalGasCost = GetTotalGasCost(safeTxGas, baseGas, gasPrice);
			var totalValue = BigInteger.Parse(txValue);
			if (walletBalance.Value < BigInteger.Add(totalGasCost, totalValue))
				throw new Exception("Not enough balance to send this value and pay for gas");
		}

		private static BigInteger GetTotalGasCost(BigInteger safeTxGas, BigInteger baseGas, BigInteger gasPrice)
		{
			var totalGasCost = BigInteger.Add(safeTxGas, baseGas);
			return BigInteger.Multiply(totalGasCost, gasPrice);
		}

		public static async Task<BigInteger> EstimateTransactionGas(IMetaTransactionData[] safeTxDataArray,
			string from, IWeb3 web3)
		{
			var safeTxGas = BigInteger.Zero;
			
			foreach (var safeTxData in safeTxDataArray)
			{
				safeTxGas += await CalculateSafeTxGas(safeTxData.data, safeTxData.to, from, web3);
			}
			
			return safeTxGas;
		}

		public static async Task<SafeTx> SetTransactionGas(SafeTx safeTxDataTyped, string from, BigInteger baseGas, IWeb3 web3)
		{
			safeTxDataTyped.safeTxGas = await EstimateTransactionGas(new IMetaTransactionData[]{safeTxDataTyped}, from, web3);
			safeTxDataTyped.baseGas = baseGas;
			safeTxDataTyped.gasPrice = await GetGasPrice(web3);

			return safeTxDataTyped;
		}

		public static async Task<BigInteger> CalculateMaxFees(string from, string to, string value, string data,
			int nonce, BigInteger baseGas, IWeb3 web3)
		{
			var safeTx = Utils.CreateSafeTx(to, value, data, nonce);
			safeTx = await SetTransactionGas(safeTx, from, baseGas, web3);
			var totalGasCost = (safeTx.safeTxGas + safeTx.baseGas) * safeTx.gasPrice;
			return totalGasCost + BigInteger.Parse(value);
		}

		private static async Task<BigInteger> CalculateSafeTxGas(string data, string to, string from, IWeb3 web3)
		{
			var ethEstimateGas = new EthEstimateGas(web3.Client);

			var transactionInput = new CallInput
			{
				Data = data,
				To = to,
				From = from
			};
			return await ethEstimateGas.SendRequestAsync(transactionInput);
		}
	}
}