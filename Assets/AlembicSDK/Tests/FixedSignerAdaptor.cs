using System.Threading.Tasks;
using AlembicSDK.Scripts.Adapters.Interfaces;
using AlembicSDK.Scripts.Tools.Signers;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;

namespace AlembicSDK.Tests
{
	// This is a test class, it is not used in the project
	public class FixedSignerAdaptor : IAuthAdaptor
	{
		private readonly Account _account;
		private readonly Signer _signer;

		private UserInfo _userInfo;
		private Web3Auth _web3Auth;

		public FixedSignerAdaptor(string chainId, string privateKey)
		{
			ChainId = chainId;
			var ethEcKey = new EthECKey(privateKey);
			_signer = new Signer(ethEcKey);
			_account = new Account(privateKey);
		}

		public string ChainId { get; }

		public Task Connect()
		{
			return Task.CompletedTask;
		}

		public Task Logout()
		{
			return Task.CompletedTask;
		}

		public Account GetAccount()
		{
			return _account;
		}

		public Signer GetSigner()
		{
			return _signer;
		}

		public UserInfo GetUserInfos()
		{
			return null;
		}
	}
}