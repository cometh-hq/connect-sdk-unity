using ComethSDK.Scripts.Types;

namespace ComethSDK.Scripts.HTTP.Responses
{
	public class GetWalletInfosResponse
	{
		public bool success { get; set; }
		public WalletInfos walletInfos { get; set; }
	}
}