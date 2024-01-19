using ComethSDK.Scripts.Types;

namespace ComethSDK.Scripts.HTTP.RequestBodies
{
	public class NewSignerRequestBody
	{
		public string walletAddress { get; set; }
		public string signerAddress { get; set; }
		public DeviceData deviceData { get; set; }
		public string type { get; set; }
		public string publicKeyId { get; set; }
		public string publicKeyX { get; set; }
		public string publicKeyY { get; set; }
	}
}