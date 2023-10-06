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

		public static async Task VerifyHasEnoughBalance(string from, string to, string value, string data, int nonce,
			IWeb3 web3)
		{
			var walletBalance = await web3.Eth.GetBalance.SendRequestAsync(from);
			var totalGasCost = await CalculateMaxFees(from, to, value, data, nonce, web3);
			if (walletBalance.Value < totalGasCost)
				throw new Exception("Not enough balance to send this value and pay for gas");
		}

		public static async Task<GasEstimates> EstimateTransactionGas(ISafeTransactionDataPartial safeTxData,
			string from, IWeb3 web3)
		{
			var safeTxGas = safeTxData.safeTxGas;
			safeTxGas += await CalculateSafeTxGas(safeTxData.data, safeTxData.to, from, web3);

			var gasPrice = safeTxData.gasPrice;
			gasPrice += await GetGasPrice(web3);

			return new GasEstimates { safeTxGas = safeTxGas, baseGas = BASE_GAS, gasPrice = gasPrice };
		}

		public static async Task<SafeTx> SetTransactionGas(SafeTx safeTxDataTyped, string from, IWeb3 web3)
		{
			var gasEstimates = await EstimateTransactionGas(safeTxDataTyped, from, web3);
			safeTxDataTyped.safeTxGas = gasEstimates.safeTxGas;
			safeTxDataTyped.baseGas = gasEstimates.baseGas;
			safeTxDataTyped.gasPrice = gasEstimates.gasPrice;

			return safeTxDataTyped;
		}

		public static async Task<BigInteger> CalculateMaxFees(string from, string to, string value, string data,
			int nonce, IWeb3 web3)
		{
			var safeTx = Utils.CreateSafeTx(to, value, data, nonce);
			safeTx = await SetTransactionGas(safeTx, from, web3);
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