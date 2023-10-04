using System.Collections.Generic;
using ComethSDK.Scripts.Types;
using Nethereum.ABI.EIP712;

namespace ComethSDK.Scripts.HTTP.RequestBodies
{
	public class SignTypedDataWithComethAuthBody
	{
		public DomainWithChainIdAndVerifyingContractLowerCase domain { get; set; }
		public IDictionary<string, MemberDescription[]> types { get; set; }
		public IDictionary<string, object> value { get; set; }
	}
}