using Nethereum.ABI.FunctionEncoding.Attributes;

namespace ComethSDK.Scripts.Types.MessageTypes
{
	[Struct("SafeMessage")]
	public class SafeMessage
	{
		[Parameter("bytes", "message")] public string message { get; set; }
	}
}