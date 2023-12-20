using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ComethSDK.Scripts.Services
{
	public static class CryptoService
	{
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
		
		public static async Task<byte[]> DecryptAESCBC(byte[] encryptionKey, byte[] iv, byte[] data)
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

		public static byte[] GetRandomIV()
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