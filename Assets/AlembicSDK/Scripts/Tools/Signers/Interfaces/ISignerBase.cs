using System.Collections.Generic;
using System.Threading.Tasks;
using AlembicSDK.Scripts.Types.MessageTypes;
using Nethereum.ABI.EIP712;

namespace AlembicSDK.Scripts.Tools.Signers
{
	public interface ISignerBase
	{
		public string SignTypedData<T, TDomain>(T message, TypedData<TDomain> typedData);

		public Task<string> SignTypedData(DomainWithChainIdAndVerifyingContract domain,
			Dictionary<string, MemberDescription[]> types, SafeTx value);
	}
}