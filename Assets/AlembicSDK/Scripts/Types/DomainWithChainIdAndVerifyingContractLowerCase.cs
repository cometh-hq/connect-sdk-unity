using Nethereum.ABI.EIP712;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace AlembicSDK.Scripts.Types
{
	[Struct("EIP712Domain")]
	public class DomainWithChainIdAndVerifyingContractLowerCase : IDomain
	{
		[Parameter("uint256", "chainId", 1)]
		public virtual string chainId { get; set; }

		[Parameter("address", "verifyingContract", 2)]
		public virtual string verifyingContract { get; set; }
	}
}