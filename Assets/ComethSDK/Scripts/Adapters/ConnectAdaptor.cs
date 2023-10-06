using System;
using System.Threading.Tasks;
using ComethSDK.Scripts.Adapters.Interfaces;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Tools;
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

		private string _account;
		private API _api;

		private ConnectionSigning _connectionSigning;
		private EthECKey _ethEcKey;
		private Signer _signer;

		private void Awake()
		{
			if(chainId == 0)
				throw new Exception("ChainId is not set");
			if(string.IsNullOrEmpty(apiKey))
				throw new Exception("ApiKey is not set");
			if(string.IsNullOrEmpty(baseUrl))
				throw new Exception("BaseUrl is not set");
			
			if (!Utils.IsNetworkSupported(chainId.ToString())) throw new Exception("This network is not supported");
			ChainId = chainId.ToString();
			
			_api = new API(apiKey, chainId);
			_connectionSigning = new ConnectionSigning(chainId, apiKey, baseUrl);
		}

		public string ChainId { get; private set; }

		public async Task Connect(string burnerAddress)
		{
			var privateKey = "";
			if (!string.IsNullOrEmpty(burnerAddress))
			{
				await VerifyWalletAddress(burnerAddress);
				privateKey = PlayerPrefs.GetString( $"cometh-connect-{burnerAddress}", null);
			}
			
			var walletAddress = "";

			if (string.IsNullOrEmpty(privateKey))
			{
				var ethEcKey = EthECKey.GenerateKey();
				privateKey = ethEcKey.GetPrivateKey();
				walletAddress = await _api.GetWalletAddress(ethEcKey.GetPublicAddress());
				Debug.Log("EthEC Address = "+walletAddress);
				PlayerPrefs.SetString($"cometh-connect-{walletAddress}", privateKey);
			}

			_ethEcKey = new EthECKey(privateKey);
			_signer = new Signer(_ethEcKey);
			var eoa = new Account(privateKey);
			_account = eoa.Address;

			if (string.IsNullOrEmpty(walletAddress))
			{
				walletAddress = await GetWalletAddress();
			}
			
			await _connectionSigning.SignAndConnect(walletAddress, GetSigner());
		}

		private async Task VerifyWalletAddress(string walletAddress)
		{
			WalletInfos connectWallet;
			try
			{
				connectWallet = await _api.GetWalletInfos(walletAddress);
			}
			catch
			{
				throw new Exception("Invalid address format");
			}

			if (connectWallet == null)
			{
				throw new Exception("Wallet does not exist");
			}
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

		private async Task<string> GetWalletAddress()
		{
			var ownerAddress = GetAccount();
			if (string.IsNullOrEmpty(ownerAddress)) throw new Exception("No owner address found");
			return await _api.GetWalletAddress(ownerAddress);
		}
	}
}