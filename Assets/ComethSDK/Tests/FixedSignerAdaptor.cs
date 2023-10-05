using System.Threading.Tasks;
using ComethSDK.Scripts.Adapters.Interfaces;
using ComethSDK.Scripts.Tools.Signers;
using ComethSDK.Scripts.Tools.Signers.Interfaces;
using ComethSDK.Scripts.Types;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;

namespace ComethSDK.Tests
{
	// This is a test class, it is not used in the project
	public class FixedSignerAdaptor : IAuthAdaptor
	{
		private readonly string _account;
		private readonly Signer _signer;

		public FixedSignerAdaptor(string chainId, string privateKey)
		{
			ChainId = chainId;
			var ethEcKey = new EthECKey(privateKey);
			_signer = new Signer(ethEcKey);
			var eoa = new Account(privateKey);
			_account = eoa.Address;
		}

		public string ChainId { get; }

		public Task Connect(string burnerAddress)
		{
			return Task.CompletedTask;
		}

		public Task Logout()
		{
			return Task.CompletedTask;
		}

		public string GetAccount()
		{
			return _account;
		}

		public ISignerBase GetSigner()
		{
			return _signer;
		}

		public UserInfos GetUserInfos()
		{
			return null;
		}
	}
}