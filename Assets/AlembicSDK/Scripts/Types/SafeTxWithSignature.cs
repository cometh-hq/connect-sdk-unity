namespace AlembicSDK.Scripts.Types
{
	public class SafeTxWithSignature
	{
		public string to { get; set; }
		public string value { get; set; }
		public string data { get; set; }
		public OperationType? operation { get; set; }
		public string safeTxGas { get; set; }
		public string baseGas { get; set; }
		public string gasPrice { get; set; }
		public string gasToken { get; set; }
		public string refundReceiver { get; set; }
		public string nonce { get; set; }
		public string signatures { get; set; }
	}
}