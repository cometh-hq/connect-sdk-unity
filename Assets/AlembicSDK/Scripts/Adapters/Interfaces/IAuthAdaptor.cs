using System.Threading.Tasks;
using AlembicSDK.Scripts.Tools.Signers.Interfaces;

namespace AlembicSDK.Scripts.Adapters.Interfaces
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