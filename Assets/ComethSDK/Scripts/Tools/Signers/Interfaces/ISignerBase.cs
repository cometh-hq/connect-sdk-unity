using Nethereum.ABI.EIP712;

namespace ComethSDK.Scripts.Tools.Signers.Interfaces
{
	public interface ISignerBase
	{
		public string SignTypedData<T, TDomain>(T message, TypedData<TDomain> typedData);
	}
}