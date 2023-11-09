using System.Numerics;
using ComethSDK.Scripts.Enums;

namespace ComethSDK.Scripts.Interfaces
{
	public interface ISafeTransactionDataPartial : IMetaTransactionData
	{
		OperationType? operation { get; }
		BigInteger safeTxGas { get; }
		BigInteger baseGas { get; }
		BigInteger gasPrice { get; }
		string gasToken { get; }
		string refundReceiver { get; }
		string nonce { get; }
	}
}