using System.Numerics;
using ComethSDK.Scripts.Interfaces;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace ComethSDK.Scripts.Types.MessageTypes
{
	[Struct("SafeTx")]
	public class SafeTx : ISafeTransactionDataPartial
	{
		[Parameter("address", "to")] public string to { get; set; }
		[Parameter("uint256", "value", 2)] public string value { get; set; }
		[Parameter("bytes", "data", 3)] public string data { get; set; }
		[Parameter("uint8", "operation", 4)] public OperationType? operation { get; set; }
		[Parameter("uint256", "safeTxGas", 5)] public BigInteger safeTxGas { get; set; }
		[Parameter("uint256", "baseGas", 6)] public BigInteger baseGas { get; set; }
		[Parameter("uint256", "gasPrice", 7)] public BigInteger gasPrice { get; set; }
		[Parameter("address", "gasToken", 8)] public string gasToken { get; set; }

		[Parameter("address", "refundReceiver", 9)]
		public string refundReceiver { get; set; }

		[Parameter("uint256", "nonce", 10)] public string nonce { get; set; }
	}
}