using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlembicSDK.Scripts.Tools.Signers.Interfaces;
using Nethereum.ABI.EIP712;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;

namespace AlembicSDK.Scripts.Tools.Signers
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

		public Task<string> SignTypedData(DomainWithChainIdAndVerifyingContract domain,
			IDictionary<string, MemberDescription[]> types, IDictionary<string, object> value)
		{
			throw new NotImplementedException();
		}
	}
}