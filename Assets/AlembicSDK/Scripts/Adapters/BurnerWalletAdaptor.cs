using System.Threading.Tasks;
using AlembicSDK.Scripts.Interfaces;
using AlembicSDK.Scripts.Tools.Signers;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using UnityEngine;

namespace AlembicSDK.Scripts.Adapters
{
	public class BurnerWalletAdaptor : MonoBehaviour, IAuthAdaptor
	{
		[SerializeField] private int chainId;

		private Account _account;
		private EthECKey _ethEcKey;
		private Signer _signer;

		private void Awake()
		{
			ChainId = chainId.ToString();
		}

		public string ChainId { get; private set; }

		public Task Connect()
		{
			//Get current private key from PlayerPrefs
			var privateKey = PlayerPrefs.GetString("privateKey", null);

			if (string.IsNullOrEmpty(privateKey))
			{
				var ethEcKey = EthECKey.GenerateKey();
				privateKey = ethEcKey.GetPrivateKey();
				PlayerPrefs.SetString("privateKey", privateKey);
			}

			_ethEcKey = new EthECKey(privateKey);
			_signer = new Signer(_ethEcKey);
			_account = new Account(privateKey);

			return Task.CompletedTask;
		}

		public Task Logout()
		{
			PlayerPrefs.DeleteKey("privateKey");
			_account = null;
			_ethEcKey = null;
			_signer = null;
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