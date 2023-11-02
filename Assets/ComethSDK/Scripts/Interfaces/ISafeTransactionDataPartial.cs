using System.Numerics;
using ComethSDK.Scripts.Types;

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