using System.Threading.Tasks;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Tools.Signers;
using Nethereum.Signer;
using UnityEngine;

namespace ComethSDK.Scripts.Services
{
	public static class BurnerWalletService
	{
		public static async Task<Signer> CreateOrGetSigner(string token, string userId, string walletAddress, API api, string provider)
		{
			if (string.IsNullOrEmpty(userId))
			{
				Debug.LogError("No UserID Found");
				return null;
			}

			var storagePrivateKey = PlayerPrefs.GetString("cometh-connect-"+userId, null);

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

			var isOwner = await SafeService.IsSigner(storageSigner.GetAddress(), walletAddress, provider ,api);

			if (!isOwner)
			{
				Debug.LogError("New Domain detected. You need to add that domain as signer.");
				return null;
			}
				
			return storageSigner;
		}

		private static Signer CreateNewSignerAndSavePrivateKey(string userId)
		{
			var ethEcKey = EthECKey.GenerateKey();
			var privateKey = ethEcKey.GetPrivateKey();
			PlayerPrefs.SetString("cometh-connect-"+userId, privateKey);
			return new Signer(ethEcKey);
		}
	}
}