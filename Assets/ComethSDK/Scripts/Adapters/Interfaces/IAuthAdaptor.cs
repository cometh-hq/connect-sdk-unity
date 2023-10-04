using System.Threading.Tasks;
using ComethSDK.Scripts.Tools.Signers.Interfaces;

namespace ComethSDK.Scripts.Adapters.Interfaces
{
	public interface IAuthAdaptor
	{
		public string ChainId { get; }

		public Task Connect();
		public Task Logout();
		public string GetAccount();
		public ISignerBase GetSigner();
		public UserInfo GetUserInfos();
	}
}