using ComethSDK.Scripts.Types;

namespace ComethSDK.Scripts.HTTP.RequestBodies
{
	public class ConnectToComethWalletBody
	{
		public SiweMessageLowerCase message { get; set; }
		public string signature { get; set; }
		public string walletAddress { get; set; }
	}
}