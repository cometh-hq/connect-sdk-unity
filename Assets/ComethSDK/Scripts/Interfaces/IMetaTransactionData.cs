using ComethSDK.Scripts.Types;

namespace ComethSDK.Scripts.Interfaces
{
	public interface IMetaTransactionData
	{
		string to { get; set; }
		string value { get; set; }
		string data { get; set; }
		OperationType? operation { get; set;}
	}
}