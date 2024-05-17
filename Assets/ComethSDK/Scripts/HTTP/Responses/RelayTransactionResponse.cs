namespace ComethSDK.Scripts.HTTP.Responses
{
	public class RelayTransactionResponse
	{
		public bool success { get; set; }
		public string safeTxHash { get; set; }
		public string error { get; set; }
	}
}