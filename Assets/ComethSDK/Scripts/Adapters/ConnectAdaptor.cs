using System;
using System.Threading.Tasks;
using ComethSDK.Scripts.Adapters.Interfaces;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Tools.Signers;
using ComethSDK.Scripts.Tools.Signers.Interfaces;
using ComethSDK.Scripts.Types;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using UnityEngine;

namespace ComethSDK.Scripts.Adapters
{
	public class ConnectAdaptor : MonoBehaviour, IAuthAdaptor
	{
		[SerializeField] private int chainId;
		[SerializeField] private string apiKey;
		[SerializeField] private string baseUrl;
		
		private ConnectionSigning _connectionSigning;

		private string _account;
		private EthECKey _ethEcKey;
		private Signer _signer;
		private API _api;

		private void Awake()
		{
			ChainId = chainId.ToString();
			_api = new API(apiKey, chainId);
			_connectionSigning = new ConnectionSigning(chainId, apiKey, baseUrl);
		}

		public string ChainId { get; private set; }

		public async Task Connect()
		{
			//Get current private key from PlayerPrefs
			var privateKey = PlayerPrefs.GetString("burner-wallet-private-key", null);

			if (string.IsNullOrEmpty(privateKey))
			{
				var ethEcKey = EthECKey.GenerateKey();
				privateKey = ethEcKey.GetPrivateKey();
				PlayerPrefs.SetString("burner-wallet-private-key", privateKey);
			}

			_ethEcKey = new EthECKey(privateKey);
			_signer = new Signer(_ethEcKey);
			var eoa = new Account(privateKey);
			_account = eoa.Address;

			var walletAddress = await GetWalletAddress();
			await _connectionSigning.SignAndConnect(walletAddress, GetSigner());
		}

		private async Task<string> GetWalletAddress()
		{
			var ownerAddress = GetAccount();
			if(string.IsNullOrEmpty(ownerAddress)) throw new Exception("No owner address found");
			return await _api.GetWalletAddress(ownerAddress);
		}

		public Task Logout()
		{
			PlayerPrefs.DeleteKey("privateKey");
			_account = null;
			_ethEcKey = null;
			_signer = null;
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