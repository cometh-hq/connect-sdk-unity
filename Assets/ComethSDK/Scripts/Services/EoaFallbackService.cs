using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace ComethSDK.Scripts.Services
{
	 
	public static class EoaFallbackService
	{
		private const int Pbkdf2Iterations = 10_000;
		public static async Task<(string encryptedPrivateKey, string iv)> EncryptEoaFallback(string walletAddress, string privateKey, string salt)
		{
			var encodedWalletAddress = Encoding.UTF8.GetBytes(walletAddress);
			var encodedSalt = Encoding.UTF8.GetBytes(salt);
			
			var encryptionKey = await Pbkdf2(encodedWalletAddress, encodedSalt, Pbkdf2Iterations);
			
			var encodedPrivateKey = Encoding.UTF8.GetBytes(privateKey);

			var iv = GetRandomIV();
			
			var encryptedPrivateKey = await EncryptAESCBC(encryptionKey, iv, encodedPrivateKey);
			
			return (
				encryptedPrivateKey: Convert.ToBase64String(encryptedPrivateKey),
				iv: Convert.ToBase64String(iv)
			);
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
	}
}