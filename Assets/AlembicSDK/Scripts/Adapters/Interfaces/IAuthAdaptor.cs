#nullable enable
using System.Threading.Tasks;
using AlembicSDK.Scripts.Tools.Signers;
using Nethereum.Web3.Accounts;

namespace AlembicSDK.Scripts.Adapters.Interfaces
{
	public interface IAuthAdaptor
	{
		public string ChainId { get; }

		public Task Connect();
		public Task Logout();
		public Account GetAccount();
		public Signer GetSigner();
		public UserInfo GetUserInfos();
	}
}