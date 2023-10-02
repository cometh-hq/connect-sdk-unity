using System.Threading.Tasks;
using AlembicSDK.Scripts.HTTP;
using AlembicSDK.Scripts.Tools.Signers;
using Nethereum.Signer;
using UnityEngine;

namespace AlembicSDK.Scripts.Services
{
	public static class BurnerWalletService
	{
		public static async Task<Signer> CreateOrGetSigner(string token, string userId, string walletAddress, API api)
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
				await api.InitWalletForUserID(token, "owner address"); //TODO: Get owner address from somewhere
				
				return newSigner;
			}

			if (string.IsNullOrEmpty(storagePrivateKey))
			{
				Debug.LogError("New Domain detected. You need to add that domain as signer.");
				return null;
			}
				
			var storageSigner = new Signer(new EthECKey(storagePrivateKey));

			var isOwner = false;

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