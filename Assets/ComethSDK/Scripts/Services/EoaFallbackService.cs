using System;
using System.IO;
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
				iv = Convert.ToBase64String(iv),
				iterations = Constants.PBKDF2_ITERATIONS
			};
		}

		public static async Task<string> DecryptEoaFallback(
			string walletAddress, byte[] encryptedPrivateKey, byte[] iv, int iterations, string salt)
		{
			var encodedWalletAddress = Encoding.UTF8.GetBytes(walletAddress);
			var encodedSalt = Encoding.UTF8.GetBytes(salt);

			var encryptionKey = await CryptoService.Pbkdf2(
				encodedWalletAddress,
				encodedSalt,
				iterations
			);

			var privateKey = await CryptoService.DecryptAESCBC(encryptionKey, iv, encryptedPrivateKey);

			return Encoding.UTF8.GetString(privateKey);
		}

		public static async Task<Signer> GetSigner(string walletAddress, string encryptionSalt)
		{
			var storagePrivateKey = await GetSignerLocalStorage(walletAddress, encryptionSalt);

			if (string.IsNullOrEmpty(storagePrivateKey))
			{
				throw new SignerInvalidException($"Storage private key is empty for address: {walletAddress}.");
			}

			var storageSigner = new Signer(new EthECKey(storagePrivateKey));

			return storageSigner;
		}

		public static async Task<bool> CheckSignerIsOwner(Signer signer, API api, string rpcUrl, string walletAddress)
		{
			var isOwner = await SafeService.IsSigner(signer.GetAddress(), walletAddress, rpcUrl, api);

			if (!isOwner)
			{
				throw new SignerUnauthorizedException("New Domain detected. You need to add that domain as signer.");
			}

			return true;
		}

		public static async Task<string> GetSignerLocalStorage(string walletAddress, string encryptionSalt)
		{
			EncryptionData localStorageV2;

			try
			{
				localStorageV2 = SaveLoadPersistentData.Load(Constants.DEFAULT_DATA_FOLDER, walletAddress);
			}
			catch (Exception e)
			{
				if (e.GetType() == typeof(DirectoryNotFoundException) || e.GetType() == typeof(FileNotFoundException))
				{
					throw new SignerNotFoundException($"No signer available for this address: {walletAddress}.");
				}
				else
				{
					throw new SignerInvalidException($"Signer is invalid for this address: {walletAddress}.");
				}
			}

			if (localStorageV2 == null) return null;

			if (string.IsNullOrEmpty(localStorageV2.encryptedPrivateKey) && string.IsNullOrEmpty(localStorageV2.iv))
				throw new SignerInvalidException("Local storage exists but no encryptedPrivateKey and ivs were inside.");

			if (string.IsNullOrEmpty(localStorageV2.encryptedPrivateKey))
				throw new SignerInvalidException("Local storage exists but no encryptedPrivateKey was inside.");

			if (string.IsNullOrEmpty(localStorageV2.iv))
				throw new SignerInvalidException("Local storage exists but no iv was inside.");

			encryptionSalt = Utils.GetEncryptionSaltOrDefault(encryptionSalt);

			int iterations = localStorageV2.iterations;

			// Handle legacy iterations
			if (iterations != Constants.PBKDF2_ITERATIONS)
			{
				iterations = 1000000;
			}

			var privateKey = await DecryptEoaFallback(
				walletAddress,
				Convert.FromBase64String(localStorageV2.encryptedPrivateKey),
				Convert.FromBase64String(localStorageV2.iv),
				iterations,
				encryptionSalt);

			// Rewrite the storage with the new iterations
			if (iterations != Constants.PBKDF2_ITERATIONS)
			{
				Debug.LogWarning("Updating iterations to " + Constants.PBKDF2_ITERATIONS);
				await SetSignerLocalStorage(walletAddress, new Signer(new EthECKey(privateKey)), encryptionSalt);
			}

			return privateKey;
		}

		public static async Task SetSignerLocalStorage(string walletAddress, Signer signer, string encryptionSalt)
		{
			encryptionSalt = Utils.GetEncryptionSaltOrDefault(encryptionSalt);
			var encryptedData = await EncryptEoaFallback(walletAddress, signer.GetPrivateKey(), encryptionSalt);
			SaveLoadPersistentData.Save(encryptedData, Constants.DEFAULT_DATA_FOLDER, walletAddress);
		}

		// reset signer

		public static async Task<(Signer signer, string walletAddress)> CreateSigner(API api, string walletAddress = "",
			string encryptionSalt = "")
		{
			if (SaveLoadPersistentData.FileExists(Constants.DEFAULT_DATA_FOLDER, walletAddress))
			{
				throw new Exception($"Cannot create signer: file {walletAddress} already exists");
			}

			var signer = CreateRandomWallet();

			if (string.IsNullOrEmpty(encryptionSalt)) encryptionSalt = Constants.DEFAULT_ENCRYPTION_SALT;

			if (!string.IsNullOrEmpty(walletAddress))
			{
				await SetSignerLocalStorage(walletAddress, signer, encryptionSalt);
				return (signer, walletAddress);
			}

			var predictedWalletAddress = await api.GetWalletAddress(signer.GetAddress());
			if (string.IsNullOrEmpty(predictedWalletAddress)) throw new Exception("Error in GetWalletAddress");

			await SetSignerLocalStorage(predictedWalletAddress, signer,
				encryptionSalt);

			return (signer, predictedWalletAddress);
		}

		private static Signer CreateRandomWallet()
		{
			var ethEcKey = EthECKey.GenerateKey();
			return new Signer(ethEcKey);
		}
	}
}