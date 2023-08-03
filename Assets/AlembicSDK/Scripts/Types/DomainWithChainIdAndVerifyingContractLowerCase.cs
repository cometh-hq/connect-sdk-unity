using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace AlembicSDK.Scripts.Types
{
	[Struct("EIP712Domain")]
	public class DomainWithChainIdAndVerifyingContractLowerCase : IDomain
	{
		[Parameter("uint256", "chainId")] public virtual BigInteger? chainId { get; set; }

		[Parameter("address", "verifyingContract", 2)]
		public virtual string verifyingContract { get; set; }
	}
}