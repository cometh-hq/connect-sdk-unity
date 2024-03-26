using ComethSDK.Scripts.Tools.Signers.Interfaces;
using Nethereum.ABI.EIP712;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;

namespace ComethSDK.Scripts.Tools.Signers
{
	public class Signer : Eip712TypedDataSigner, ISignerBase
	{
		private readonly EthECKey _ethEcKey;

		public Signer(EthECKey ethEcKey)
		{
			_ethEcKey = ethEcKey;
		}

		public string SignTypedData<T, TDomain>(
			T message,
			TypedData<TDomain> typedData)
		{
			return SignTypedDataV4(message, typedData, _ethEcKey);
		}

		public string GetAddress()
		{
			return _ethEcKey.GetPublicAddress();
		}

		public string GetPrivateKey()
		{
			return _ethEcKey.GetPrivateKey();
		}
	}
}