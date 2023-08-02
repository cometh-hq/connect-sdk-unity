using System.Collections.Generic;
using AlembicSDK.Scripts.Types.MessageTypes;
using Nethereum.ABI.EIP712;

namespace AlembicSDK.Scripts.HTTP.RequestBodies
{
	public class SignTypedDataWithAlembicAuthBody
	{
		public DomainWithChainIdAndVerifyingContract domain { get; set; }
		public Dictionary<string, MemberDescription[]> types { get; set; }
		public SafeTx value { get; set; }
	}
}