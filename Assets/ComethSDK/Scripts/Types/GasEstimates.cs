using System.Numerics;

namespace ComethSDK.Scripts.Types
{
	public class GasEstimates
	{
		public GasEstimates(BigInteger _bigInteger, BigInteger _baseGas, BigInteger _gasPrice)
		{
			safeTxGas = _bigInteger;
			baseGas = _baseGas;
			gasPrice = _gasPrice;
		}

		public BigInteger safeTxGas { get; set; }
		public BigInteger baseGas { get; set; }
		public BigInteger gasPrice { get; set; }
	}
}