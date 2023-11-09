using ComethSDK.Scripts.Interfaces;

namespace ComethSDK.Scripts.Types
{
	public class MetaTransactionData : IMetaTransactionData
	{
		public string to { get; set; }
		public string value { get; set; }
		public string data { get; set; }
	}
}