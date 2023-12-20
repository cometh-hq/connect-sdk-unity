using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Tools.Signers;
using ComethSDK.Scripts.Types;
using Newtonsoft.Json;
using UnityEngine;

namespace ComethSDK.Scripts.Services
{
	public static class EoaFallbackService
	{
		private const int Pbkdf2Iterations = 10_000;

		public static async Task<EncryptionData> EncryptEoaFallback(string walletAddress,
			string privateKey, string salt)
		{
			var encodedWalletAddress = Encoding.UTF8.GetBytes(walletAddress);
			var encodedSalt = Encoding.UTF8.GetBytes(salt);

			var encryptionKey = await Pbkdf2(encodedWalletAddress, encodedSalt, Pbkdf2Iterations);

			var encodedPrivateKey = Encoding.UTF8.GetBytes(privateKey);

			var iv = GetRandomIV();

			var encryptedPrivateKey = await EncryptAESCBC(encryptionKey, iv, encodedPrivateKey);

			return new EncryptionData
			{
				encryptedPrivateKey = Convert.ToBase64String(encryptedPrivateKey),
				iv = Convert.ToBase64String(iv)
			};
		}

		public static async Task<byte[]> Pbkdf2(byte[] walletAddress, byte[] salt, int iterations)
		{
			using (var deriveBytes = new Rfc2898DeriveBytes(walletAddress, salt, iterations, HashAlgorithmName.SHA256))
			{
				return await Task.Run(() => deriveBytes.GetBytes(32));
			}
		}

		public static async Task<byte[]> EncryptAESCBC(byte[] encryptionKey, byte[] iv, byte[] data)
		{
			using (var aes = Aes.Create())
			{
				aes.Key = encryptionKey;
				aes.IV = iv;
				aes.Mode = CipherMode.CBC;

				using (var encryptor = aes.CreateEncryptor())
				using (var ms = new MemoryStream())
				{
					using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
					{
						await cryptoStream.WriteAsync(data, 0, data.Length);
						cryptoStream.FlushFinalBlock();
					}

					return ms.ToArray();
				}
			}
		}

		public static async Task<string> DecryptEoaFallback(
			string walletAddress, byte[] encryptedPrivateKey, byte[] iv, string salt)
		{
			var encodedWalletAddress = Encoding.UTF8.GetBytes(walletAddress);
			var encodedSalt = Encoding.UTF8.GetBytes(salt);

			var encryptionKey = await Pbkdf2(encodedWalletAddress, encodedSalt, Pbkdf2Iterations);

			var privateKey = await DecryptAESCBC(encryptionKey, iv, encryptedPrivateKey);

			return Encoding.UTF8.GetString(privateKey);
		}

		private static async Task<byte[]> DecryptAESCBC(byte[] encryptionKey, byte[] iv, byte[] data)
		{
			using (var aes = Aes.Create())
			{
				aes.Key = encryptionKey;
				aes.IV = iv;
				aes.Mode = CipherMode.CBC;

				using (var decryptor = aes.CreateDecryptor())
				using (var ms = new MemoryStream())
				{
					using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
					{
						await cryptoStream.WriteAsync(data, 0, data.Length);
						cryptoStream.FlushFinalBlock();
					}

					return ms.ToArray();
				}
			}
		}

		private static byte[] GetRandomIV()
		{
			using (var rng = new RNGCryptoServiceProvider())
			{
				var iv = new byte[16];
				rng.GetBytes(iv);
				return iv;
			}
		}

		public static async Task<Signer> GetSigner(API api, string provider, string walletAddress,
			string encryptionSalt)
		{
			//var storagePrivateKey = await

			return null;
		}

		public static async Task<string> GetSignerLocalStorage(string walletAddress, string encryptionSalt)
		{
			if (string.IsNullOrEmpty(encryptionSalt))
			{
				encryptionSalt = Constants.DEFAULT_ENCRYPTION_SALT;
			}

			var localStorageV1 = PlayerPrefs.GetString($"cometh-connect-{walletAddress}");

			var localStorageV2 = Application.persistentDataPath; //TODO: get from persistent data path

			if (!string.IsNullOrEmpty(localStorageV1))
			{
				var privateKey = localStorageV1;

				await SetSignerLocalStorage(walletAddress, privateKey, encryptionSalt);
				PlayerPrefs.DeleteKey($"cometh-connect-{walletAddress}");

				return privateKey;
			}

			if (!string.IsNullOrEmpty(localStorageV2))
			{
				var encryptedData =
					SaveLoadPersistentData.Load("connect",
						walletAddress);
				
				var privateKey = await DecryptEoaFallback(
					walletAddress,
					Convert.FromBase64String(encryptedData.encryptedPrivateKey),
					Convert.FromBase64String(encryptedData.iv),
					encryptionSalt);

				return privateKey;
			}

			return null;
		}

		public static async Task SetSignerLocalStorage(string walletAddress, string privateKey, string encryptionSalt)
		{
			var encryptedData = await EncryptEoaFallback(walletAddress, privateKey, encryptionSalt);

			/*
			// Encode data to JSON string
			var jsonData = JsonUtility.ToJson((encryptedPrivateKey, iv));
			// Create the full file path
			var filePath = Path.Combine(dataPath, walletAddress);
			// Write data to the file system
			await File.WriteAllTextAsync(filePath, jsonData);*/
			SaveLoadPersistentData.Save(encryptedData, "connect", walletAddress);
		}
	}
}