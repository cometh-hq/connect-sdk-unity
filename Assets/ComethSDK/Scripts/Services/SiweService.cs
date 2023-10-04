using System;
using Nethereum.Siwe.Core;

namespace ComethSDK.Scripts.Services
{
	public static class SiweService
	{
		public static SiweMessage CreateMessage(string address, string nonce, string chainId, Uri uri)
		{
			var domain = uri.Host;
			var origin = uri.Scheme + "://" + uri.Host;
			const string statement = "Sign in with Ethereum to Cometh";

			var message = new SiweMessage
			{
				Domain = domain,
				Address = address,
				Statement = statement,
				Uri = origin,
				Version = "1",
				ChainId = chainId,
				Nonce = nonce
			};

			message.SetIssuedAtNow();
			return message;
		}
	}
}