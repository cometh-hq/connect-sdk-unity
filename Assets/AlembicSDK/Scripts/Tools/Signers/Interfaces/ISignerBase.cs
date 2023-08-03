using System.Collections.Generic;
using System.Threading.Tasks;
using AlembicSDK.Scripts.Types.MessageTypes;
using Nethereum.ABI.EIP712;

namespace AlembicSDK.Scripts.Tools.Signers.Interfaces
{
	public interface ISignerBase
	{
		public string SignTypedData<T, TDomain>(T message, TypedData<TDomain> typedData);

		public Task<string> SignTypedData(DomainWithChainIdAndVerifyingContract domain,
			IDictionary<string, MemberDescription[]> types, IDictionary<string, object> value);
	}
}