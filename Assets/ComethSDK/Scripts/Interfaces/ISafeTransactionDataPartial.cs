using System.Numerics;

namespace ComethSDK.Scripts.Interfaces
{
	public interface ISafeTransactionDataPartial : IMetaTransactionData
	{
		BigInteger safeTxGas { get; }
		BigInteger baseGas { get; }
		BigInteger gasPrice { get; }
		string gasToken { get; }
		string refundReceiver { get; }
		string nonce { get; }
	}
}