﻿using System;
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

		private string _baseUrl;
		private readonly string _encryptionSalt;
		private readonly string _provider;
		private Signer _signer;
		private string _walletAddress;

		public ConnectAdaptor(int chainId, string apiKey, string baseUrl = "", string encryptionSalt = "")
		{
			if (chainId == 0)
				throw new Exception("ChainId is not set");
			if (string.IsNullOrEmpty(apiKey))
				throw new Exception("ApiKey is not set");

			if (!Utils.IsNetworkSupported(chainId.ToString())) throw new Exception("This network is not supported");
			ChainId = chainId.ToString();

			_baseUrl = baseUrl;
			_encryptionSalt = Utils.GetEncryptionSaltOrDefault(encryptionSalt);
			_provider = Constants.GetNetworkByChainID(ChainId).RPCUrl;
			_api = string.IsNullOrEmpty(baseUrl) ? new API(apiKey, chainId) : new API(apiKey, chainId, baseUrl);
		}

		public string ChainId { get; }

		public async Task Connect(string burnerAddress = "")
		{
			if (!string.IsNullOrEmpty(burnerAddress))
			{
				await EoaFallbackService.MigrateV1Keys(burnerAddress, _encryptionSalt);
				_signer = await EoaFallbackService.GetSigner(_api, _provider, burnerAddress, _encryptionSalt);
				_walletAddress = burnerAddress;
			}
			else
			{
				var (signer, walletAddress) =
					await EoaFallbackService.CreateSigner(_api, encryptionSalt: _encryptionSalt);
				_signer = signer;
				_walletAddress = walletAddress;
			}
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

		public async Task<string> GetWalletAddress()
		{
			if (string.IsNullOrEmpty(_walletAddress)) throw new Exception("No wallet Instance found");
			return _walletAddress;
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
			var walletAddress = await GetWalletAddress();
			return await _api.GetNewSignerRequests(walletAddress);
		}

		private async Task<string> InitAdaptorWalletAddress(string address)
		{
			if (_signer == null) throw new Exception("No signer instance found");

			return string.IsNullOrEmpty(address)
				? address
				: await _api.GetWalletAddress(_signer.GetAddress());
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

			if (connectWallet == null) throw new Exception("Wallet does not exist");
		}

		private void CheckIfSignerIsSet()
		{
			if (_signer == null) throw new Exception("No signer instance found");
		}
	}
}