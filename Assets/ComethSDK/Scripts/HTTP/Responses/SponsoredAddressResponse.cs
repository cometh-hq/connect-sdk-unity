namespace ComethSDK.Scripts.HTTP.Responses
{
	public class SponsoredAddressResponse
	{
		public bool success { get; set; }
		public SponsoredAddress[] sponsoredAddresses { get; set; }

		public struct SponsoredAddress
		{
			public string targetAddress;
			public string chainId;
		}
	}
}