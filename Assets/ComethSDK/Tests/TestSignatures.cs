using System;
using System.Text;
using System.Threading.Tasks;
using CandyCoded.env;
using ComethSDK.Scripts.Core;
using ComethSDK.Scripts.Tools;
using Nethereum.Contracts.Standards.ERC1271;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Siwe.Core;
using Nethereum.Web3;
using NUnit.Framework;
using UnityEngine;

namespace ComethSDK.Tests
{
	public class TestSignatures
	{
		private const string MESSAGETOSIGN = "toto";
		private const string CHAINID = "80001";

		private const string
			PRIVATEKEY =
				"0x08aa89107bccf655b326aae1ec53fffed17c726d4d2df295510cacd215a046b8"; //USING HARDCODED PRIVATE KEY

		private const string WALLETADDRESS = "0xE0f3CcBf9d66272aE8B67891E4Edc3eff67C6bAE";

		private readonly Uri _uri = new("https://api.cometh.finance");
		private string APIKEY = "";

		[Test]
		public async Task SignMessage()
		{
			if (string.IsNullOrEmpty(APIKEY))
			{
				if (env.TryParseEnvironmentVariable("API_KEY", out string apiKey))
				{
					APIKEY = apiKey;
				}
				else
				{
					Debug.LogError("API_KEY environment variable not set");
					Assert.Fail();
				}
			}

			const string EXPECTED_SIGNATURE =
				"0x8101c1081dfb8d2b1810183fc781b518963005b896277624264fe7136c8dc07814ee308dd059bc637b7b592d69c908bdda39018871b27f6688e596aaebde08371b";

			var walletAdapter = new FixedSignerAdaptor(CHAINID, PRIVATEKEY);
			await walletAdapter.Connect("");

			var wallet = new ComethWallet(walletAdapter, APIKEY);
			await wallet.Connect("");

			var walletAddress = wallet.GetAddress();
			Assert.AreEqual(WALLETADDRESS, walletAddress);

			var signature = await wallet.SignMessage(MESSAGETOSIGN);
			Assert.AreEqual(EXPECTED_SIGNATURE, signature);

			var signatureBytes = signature.HexToByteArray();

			var ethereumMessageSigner = new EthereumMessageSigner();
			var messageBytes = Encoding.UTF8.GetBytes(MESSAGETOSIGN);
			var hashedMessage = ethereumMessageSigner.HashPrefixedMessage(messageBytes);

			var web3 = new Web3(Constants.GetNetworkByChainID(CHAINID).RPCUrl);
			var erc1271ContractService = web3.Eth.ERC1271.GetContractService(walletAddress);
			var result = await erc1271ContractService.IsValidSignatureQueryAsync(hashedMessage, signatureBytes);

			var isValid = ERC1271ContractService.IsValidSignatureOutputTheSameAsMagicValue(result);
			Assert.IsTrue(isValid);
		}

		[Test]
		public async Task SignSiweMessage()
		{
			if (string.IsNullOrEmpty(APIKEY))
			{
				if (env.TryParseEnvironmentVariable("API_KEY", out string apiKey))
				{
					APIKEY = apiKey;
				}
				else
				{
					Debug.LogError("API_KEY environment variable not set");
					Assert.Fail();
				}
			}

			const string NONCE = "PMQ4geD9r8mX8CJqV";
			const string EXPECTED_SIGNATURE =
				"0x7dadd81326227ca6e768bc5848e35a3f4a709eed8f1ccac5d893ad81942e49795b5c26315e776212f86a3a2e457d5cf1316ac7fc278fd7affc814ae89202f5721b";

			var walletAdapter = new FixedSignerAdaptor(CHAINID, PRIVATEKEY);
			await walletAdapter.Connect("");

			var wallet = new ComethWallet(walletAdapter, APIKEY);
			await wallet.Connect("");

			var walletAddress = wallet.GetAddress();
			Assert.AreEqual(WALLETADDRESS, walletAddress);

			var siweMessage = CreateMessage(walletAddress, NONCE);
			var siweMessageStr = SiweMessageStringBuilder.BuildMessage(siweMessage);

			var signature = await wallet.SignMessage(siweMessageStr);
			Assert.AreEqual(EXPECTED_SIGNATURE, signature);

			var ethereumMessageSigner = new EthereumMessageSigner();
			var web3 = new Web3(Constants.GetNetworkByChainID(CHAINID).RPCUrl);

			var isValid = await web3.Eth.ERC1271.GetContractService(siweMessage.Address)
				.IsValidSignatureAndValidateReturnQueryAsync(
					ethereumMessageSigner.HashPrefixedMessage(Encoding.UTF8.GetBytes(siweMessageStr)),
					signature.HexToByteArray());

			Assert.IsTrue(isValid);
		}

		private SiweMessage CreateMessage(string address, string nonce, string chainId = CHAINID)
		{
			var domain = _uri.Host;
			var origin = _uri.Scheme + "://" + _uri.Host;
			const string statement = "Sign in with Ethereum to Cometh";

			var message = new SiweMessage
			{
				Domain = domain,
				Address = address,
				Statement = statement,
				Uri = origin,
				Version = "1",
				ChainId = chainId,
				IssuedAt = "2023-06-08T16:50:52.1018817Z",
				ExpirationTime = "2023-06-08T17:00:52.1018817Z",
				Nonce = nonce
			};

			return message;
		}
	}
}