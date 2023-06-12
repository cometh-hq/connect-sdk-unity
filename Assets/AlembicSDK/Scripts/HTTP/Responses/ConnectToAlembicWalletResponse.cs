namespace AlembicSDK.Scripts.HTTP.Responses
{
	public class ConnectToAlembicWalletResponse
	{
		public bool success { get; set; }
		public string walletAddress { get; set; }
		public string isDeployed { get; set; }
	}
}