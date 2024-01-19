using ComethSDK.Scripts.Interfaces;
using ComethSDK.Scripts.Types;
using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;

namespace ComethSDK.Scripts.Tools
{
	public static class MultiSend
	{
		public static IMetaTransactionData EncodeMultiSendArray(IMetaTransactionData[] safeTransactionDataPartials,
			string provider, string multiSendContractAddress)
		{
			var transactionEncoded = "0x";
			foreach (var safeTransaction in safeTransactionDataPartials)
			{
				var encodedData = EncodeMultiSend(safeTransaction);
				var encodedHexData = encodedData.ToHex();
				var encodedHexDataWithoutPrefix = encodedHexData.RemoveHexPrefix();
				transactionEncoded += encodedHexDataWithoutPrefix;
			}

			var web3 = new Web3(provider);

			var contract = web3.Eth.GetContract(Constants.MULTI_SEND_ABI, multiSendContractAddress);
			var multiSendFunction = contract.GetFunction("multiSend");
			var data = multiSendFunction.GetData(transactionEncoded.HexToByteArray());

			var metaTransaction = new MetaTransactionData
			{
				to = multiSendContractAddress,
				value = "0",
				data = data
			};
			return metaTransaction;
		}

		private static byte[] EncodeMultiSend(IMetaTransactionData safeTxData)
		{
			var callData = safeTxData.data.HexToByteArray();
			var operation = new ABIValue("uint8", 0);
			var address = new ABIValue("address", safeTxData.to);
			var value = new ABIValue("uint256", safeTxData.value);
			var dataLength = new ABIValue("uint256", callData.Length);
			var data = new ABIValue("bytes", callData);

			return new ABIEncode().GetABIEncodedPacked(operation, address, value, dataLength, data);
		}
	}
}