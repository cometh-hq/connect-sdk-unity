using System;
using System.Text;
using System.Threading.Tasks;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Tools.Signers;
using ComethSDK.Scripts.Types;
using Nethereum.Signer;
using UnityEngine;

namespace ComethSDK.Scripts.Services
{
	public static class EoaFallbackService
	{
		public static async Task<EncryptionData> EncryptEoaFallback(string walletAddress,
			string privateKey, string salt)
		{
			var encodedWalletAddress = Encoding.UTF8.GetBytes(walletAddress);
			var encodedSalt = Encoding.UTF8.GetBytes(salt);

			var encryptionKey =
				await CryptoService.Pbkdf2(encodedWalletAddress, encodedSalt, Constants.PBKDF2_ITERATIONS);

			var encodedPrivateKey = Encoding.UTF8.GetBytes(privateKey);

			var iv = CryptoService.GetRandomIV();

			var encryptedPrivateKey = await CryptoService.EncryptAESCBC(encryptionKey, iv, encodedPrivateKey);

			return new EncryptionData
			{
				encryptedPrivateKey = Convert.ToBase64String(encryptedPrivateKey),
				iv = Convert.ToBase64String(iv)
			};
		}

		public static async Task<string> DecryptEoaFallback(
			string walletAddress, byte[] encryptedPrivateKey, byte[] iv, string salt)
		{
			var encodedWalletAddress = Encoding.UTF8.GetBytes(walletAddress);
			var encodedSalt = Encoding.UTF8.GetBytes(salt);

			var encryptionKey =
				await CryptoService.Pbkdf2(encodedWalletAddress, encodedSalt, Constants.PBKDF2_ITERATIONS);

			var privateKey = await CryptoService.DecryptAESCBC(encryptionKey, iv, encryptedPrivateKey);

			return Encoding.UTF8.GetString(privateKey);
		}

		public static async Task<Signer> GetSigner(API api, string provider, string walletAddress,
			string encryptionSalt)
		{
			var storagePrivateKey = await GetSignerLocalStorage(walletAddress, encryptionSalt);

			if (string.IsNullOrEmpty(storagePrivateKey))
				throw new Exception("New Domain detected. You need to add that domain as signer.");

			var storageSigner = new Signer(new EthECKey(storagePrivateKey));

			var isOwner = await SafeService.IsSigner(storageSigner.GetAddress(), walletAddress, provider, api);

			if (!isOwner) throw new Exception("New Domain detected. You need to add that domain as signer.");

			return storageSigner;
		}

		public static async Task<string> GetSignerLocalStorage(string walletAddress, string encryptionSalt)
		{
			if (string.IsNullOrEmpty(encryptionSalt)) encryptionSalt = Constants.DEFAULT_ENCRYPTION_SALT;

			var localStorageV1 = PlayerPrefs.GetString($"cometh-connect-{walletAddress}");

			if (!string.IsNullOrEmpty(localStorageV1))
			{
				var privateKey = localStorageV1;

				await SetSignerLocalStorage(walletAddress, privateKey, encryptionSalt);
				PlayerPrefs.DeleteKey($"cometh-connect-{walletAddress}");

				return privateKey;
			}

			var localStorageV2 = SaveLoadPersistentData.Load("connect",
				walletAddress);

			if (localStorageV2 == null) return null;
			if (!string.IsNullOrEmpty(localStorageV2.encryptedPrivateKey) && !string.IsNullOrEmpty(localStorageV2.iv))
			{
				var privateKey = await DecryptEoaFallback(
					walletAddress,
					Convert.FromBase64String(localStorageV2.encryptedPrivateKey),
					Convert.FromBase64String(localStorageV2.iv),
					encryptionSalt);

				return privateKey;
			}

			return null;
		}

		public static async Task SetSignerLocalStorage(string walletAddress, string privateKey, string encryptionSalt)
		{
			var encryptedData = await EncryptEoaFallback(walletAddress, privateKey, encryptionSalt);
			SaveLoadPersistentData.Save(encryptedData, "connect", walletAddress);
		}

		public static async Task<(Signer, string)> CreateSigner(API api, string walletAddress = "",
			string encryptionSalt = "")
		{
			var ethEcKey = EthECKey.GenerateKey();
			var privateKey = ethEcKey.GetPrivateKey();
			var signer = new Signer(ethEcKey);

			if (string.IsNullOrEmpty(encryptionSalt)) encryptionSalt = Constants.DEFAULT_ENCRYPTION_SALT;

			if (!string.IsNullOrEmpty(walletAddress))
			{
				await SetSignerLocalStorage(walletAddress, privateKey, encryptionSalt);
				return (signer, walletAddress);
			}

			var predictedWalletAddress = await api.InitWallet(signer.GetAddress());

			await SetSignerLocalStorage(predictedWalletAddress, privateKey, encryptionSalt);

			return (signer, predictedWalletAddress);
		}
	}
}