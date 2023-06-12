namespace AlembicSDK.Scripts.HTTP.Responses
{
	public class UserNonce
	{
		public string walletAddress { get; set; }
		public string connectionNonce { get; set; }
	}

	public class NonceResponse
	{
		public bool success { get; set; }
		public UserNonce userNonce { get; set; }
	}
}