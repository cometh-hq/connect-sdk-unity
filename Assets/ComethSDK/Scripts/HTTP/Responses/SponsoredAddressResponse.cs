namespace ComethSDK.Scripts.HTTP.Responses
{
	public class SponsoredAddressResponse
	{
		public bool success { get; set; }
		public SponsoredAddress[] sponsoredAddresses { get; set; }

		public struct SponsoredAddress
		{
			public string CustomerId;
			public string TargetAddress;
			public string ChainId;
		}
	}
}