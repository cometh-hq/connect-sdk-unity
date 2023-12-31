using System;
using System.Threading.Tasks;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Tools.Signers;
using Nethereum.Signer;
using UnityEngine;

namespace ComethSDK.Scripts.Services
{
	public static class BurnerWalletService
	{
		public static async Task<Signer> CreateSignerForUserId(string token, string userId, string walletAddress,
			API api,
			string provider)
		{
			if (string.IsNullOrEmpty(userId))
			{
				Debug.LogError("No UserID Found");
				return null;
			}

			var storagePrivateKey = PlayerPrefs.GetString("cometh-connect-" + userId, null);

			if (string.IsNullOrEmpty(walletAddress))
			{
				var newSigner = CreateNewSignerAndSavePrivateKey(userId);
				await api.InitWalletForUserID(token, newSigner.GetAddress());

				return newSigner;
			}

			if (string.IsNullOrEmpty(storagePrivateKey))
			{
				Debug.LogError("New Domain detected. You need to add that domain as signer.");
				return null;
			}

			var storageSigner = new Signer(new EthECKey(storagePrivateKey));

			var isOwner = await SafeService.IsSigner(storageSigner.GetAddress(), walletAddress, provider, api);

			if (!isOwner)
			{
				Debug.LogError("New Domain detected. You need to add that domain as signer.");
				return null;
			}

			return storageSigner;
		}

		public static async Task<Signer> CreateSigner(API api)
		{
			var ethEcKey = EthECKey.GenerateKey();
			var privateKey = ethEcKey.GetPrivateKey();
			var walletAddress = await api.InitWallet(ethEcKey.GetPublicAddress());
			PlayerPrefs.SetString($"cometh-connect-{walletAddress}", privateKey);

			return new Signer(ethEcKey);
		}

		public static async Task<Signer> GetSignerForUserId(string userId, string walletAddress, API api,
			string provider)
		{
			var storagePrivateKey = PlayerPrefs.GetString("cometh-connect-" + userId, null);
			if (string.IsNullOrEmpty(storagePrivateKey))
				throw new Exception("New Domain detected. You need to add that domain as signer.");

			var storageSigner = new Signer(new EthECKey(storagePrivateKey));

			var isOwner = await SafeService.IsSigner(storageSigner.GetAddress(), walletAddress, provider, api);

			if (!isOwner)
			{
				Debug.LogError("New Domain detected. You need to add that domain as signer.");
				return null;
			}

			return storageSigner;
		}

		public static async Task<Signer> GetSigner(string walletAddress, API api,
			string provider)
		{
			var storagePrivateKey = PlayerPrefs.GetString("cometh-connect-" + walletAddress, null);
			if (string.IsNullOrEmpty(storagePrivateKey))
				throw new Exception("New Domain detected. You need to add that domain as signer.");

			var storageSigner = new Signer(new EthECKey(storagePrivateKey));

			var isOwner = await SafeService.IsSigner(storageSigner.GetAddress(), walletAddress, provider, api);

			if (!isOwner)
				throw new Exception("New Domain detected. You need to add that domain as signer.");

			return storageSigner;
		}

		private static Signer CreateNewSignerAndSavePrivateKey(string userId)
		{
			var ethEcKey = EthECKey.GenerateKey();
			var privateKey = ethEcKey.GetPrivateKey();
			PlayerPrefs.SetString("cometh-connect-" + userId, privateKey);
			return new Signer(ethEcKey);
		}
	}
}