namespace ComethSDK.Scripts.Types
{
	public class EncryptionData
	{
		public string encryptedPrivateKey { get; set; }
		public string iv { get; set; }
		public int iterations { get; set; }
	}
}