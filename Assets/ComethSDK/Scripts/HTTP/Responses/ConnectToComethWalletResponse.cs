namespace ComethSDK.Scripts.HTTP.Responses
{
	public class ConnectToComethWalletResponse
	{
		public bool success { get; set; }
		public string walletAddress { get; set; }
		public string isDeployed { get; set; }
	}
}