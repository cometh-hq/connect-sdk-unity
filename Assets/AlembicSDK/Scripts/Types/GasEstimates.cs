using System.Numerics;

namespace AlembicSDK.Scripts.Types
{
	public class GasEstimates
	{
		public BigInteger safeTxGas { get; set; }
		public BigInteger baseGas { get; set; }
		public BigInteger gasPrice { get; set; }
	}
}