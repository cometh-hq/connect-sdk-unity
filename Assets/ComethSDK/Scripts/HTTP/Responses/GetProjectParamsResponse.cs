using ComethSDK.Scripts.Types;

namespace ComethSDK.Scripts.HTTP.Responses
{
	public class GetProjectParamsResponse
	{
		public bool success { get; set; }
		public ProjectParams projectParams { get; set; }
	}
}