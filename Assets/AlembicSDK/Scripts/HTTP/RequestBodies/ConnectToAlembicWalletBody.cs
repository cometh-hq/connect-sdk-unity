using AlembicSDK.Scripts.Types;

namespace AlembicSDK.Scripts.HTTP.RequestBodies
{
	public class ConnectToAlembicWalletBody
	{
		public SiweMessageLowerCase message { get; set; }
		public string signature { get; set; }
		public string walletAddress { get; set; }
	}
}