using ComethSDK.Scripts.HTTP.RequestBodies;

namespace ComethSDK.Scripts.HTTP.Responses
{
	public class GetNewSignerRequestsResponse
	{
		public bool success { get; set; }
		public NewSignerRequestBody[] signerRequests { get; set; }
	}
}