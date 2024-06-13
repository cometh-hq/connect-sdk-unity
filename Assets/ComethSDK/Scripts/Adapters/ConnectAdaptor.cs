using System;
using System.Threading.Tasks;
using ComethSDK.Scripts.Adapters.Interfaces;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.HTTP.RequestBodies;
using ComethSDK.Scripts.Services;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Tools.Signers;
using ComethSDK.Scripts.Tools.Signers.Interfaces;
using ComethSDK.Scripts.Types;
using Nethereum.Signer;
using UnityEngine;

namespace ComethSDK.Scripts.Adapters
{
	public class ConnectAdaptor : IAuthAdaptor
	{
		private readonly API _api;
		private readonly string _encryptionSalt;
		private readonly string _provider;

		private Signer _signer;
		private string _walletAddress;

		public ConnectAdaptor(int chainId, string apiKey, string baseUrl = "", string encryptionSalt = "",
			string provider = "")
		{
			if (chainId == 0)
				throw new Exception("ChainId is not set");
			if (string.IsNullOrEmpty(apiKey))
				throw new Exception("ApiKey is not set");

			if (!Utils.IsNetworkSupported(chainId.ToString())) throw new Exception("This network is not supported");
			ChainId = chainId.ToString();

			_encryptionSalt = Utils.GetEncryptionSaltOrDefault(encryptionSalt);
			_provider = string.IsNullOrEmpty(provider)
				? Constants.GetNetworkByChainID(ChainId).RPCUrl
				: provider;
			_api = string.IsNullOrEmpty(baseUrl) ? new API(apiKey, chainId) : new API(apiKey, chainId, baseUrl);
		}

		public string ChainId { get; }

		public async Task Connect(string walletAddress = "")
		{
			if (!string.IsNullOrEmpty(walletAddress))
				await Reconnect(walletAddress);
			else
				await CreateWallet();
		}

		public Task Logout()
		{
			CheckIfSignerIsSet();
			_signer = null;
			_walletAddress = null;
			return Task.CompletedTask;
		}

		public string GetAccount()
		{
			CheckIfSignerIsSet();
			return _signer.GetAddress();
		}

		public ISignerBase GetSigner()
		{
			CheckIfSignerIsSet();
			return _signer;
		}

		public UserInfos GetUserInfos()
		{
			CheckIfSignerIsSet();
			return new UserInfos
			{
				walletAddress = GetAccount()
			};
		}

		public string GetWalletAddress()
		{
			if (string.IsNullOrEmpty(_walletAddress)) throw new Exception("No wallet Instance found");
			return _walletAddress;
		}

		private async Task Reconnect(string walletAddress)
		{
			var wallet = await _api.GetWalletInfos(walletAddress);
			if (wallet == null) throw new Exception("Wallet does not exist");

			_signer = await EoaFallbackService.GetSigner(_api, _provider, walletAddress, _encryptionSalt);
			_walletAddress = walletAddress;
		}

		private async Task CreateWallet()
		{
			var (signer, walletAddress) =
				await EoaFallbackService.CreateSigner(_api, encryptionSalt: _encryptionSalt);
			_signer = signer;
			_walletAddress = walletAddress;

			await _api.InitWallet(signer.GetAddress());
		}

		public async Task<NewSignerRequestBody> InitNewSignerRequest(string walletAddress)
		{
			var ethEcKey = EthECKey.GenerateKey();
			var privateKey = ethEcKey.GetPrivateKey();
			_signer = new Signer(ethEcKey);
			PlayerPrefs.SetString($"cometh-connect-{walletAddress}", privateKey);

			var addNewSignerRequest = new NewSignerRequestBody
			{
				walletAddress = walletAddress,
				signerAddress = _signer.GetAddress(),
				deviceData = DeviceService.GetDeviceData(),
				type = NewSignerRequestType.BURNER_WALLET
			};

			return addNewSignerRequest;
		}

		public async Task<NewSignerRequestBody[]> GetNewSignerRequests()
		{
			var walletAddress = GetWalletAddress();
			return await _api.GetNewSignerRequests(walletAddress);
		}

		private void CheckIfSignerIsSet()
		{
			if (_signer == null) throw new Exception("No signer instance found");
		}

		public async Task<Signer> CreateNewSigner(string walletAddress)
		{
			var (signer, _) = await EoaFallbackService.CreateSigner(_api, walletAddress, _encryptionSalt);

			return signer;
		}
	}
}