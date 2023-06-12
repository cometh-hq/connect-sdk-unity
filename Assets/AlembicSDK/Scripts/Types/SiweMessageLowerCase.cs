using System.Collections.Generic;
using Nethereum.Siwe.Core;

namespace AlembicSDK.Scripts.Types
{
	public class SiweMessageLowerCase
	{
		public SiweMessageLowerCase(SiweMessage siweMessage)
		{
			domain = siweMessage.Domain;
			address = siweMessage.Address;
			statement = siweMessage.Statement;
			uri = siweMessage.Uri;
			version = siweMessage.Version;
			nonce = siweMessage.Nonce;
			issuedAt = siweMessage.IssuedAt;
			expirationTime = siweMessage.ExpirationTime;
			notBefore = siweMessage.NotBefore;
			requestId = siweMessage.RequestId;
			chainId = siweMessage.ChainId;
			resources = siweMessage.Resources;
		}

		public string domain { get; set; }

		public string address { get; set; }

		public string statement { get; set; }

		public string uri { get; set; }

		public string version { get; set; }

		public string nonce { get; set; }

		public string issuedAt { get; set; }

		public string expirationTime { get; set; }

		public string notBefore { get; set; }

		public string requestId { get; set; }

		public string chainId { get; set; }

		public List<string> resources { get; set; }
	}
}