using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Services;
using ComethSDK.Scripts.Tools.Signers;
using ComethSDK.Scripts.Tools.Signers.Interfaces;
using ComethSDK.Scripts.Types.MessageTypes;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Siwe.Core;

namespace ComethSDK.Scripts.Adapters.Interfaces
{
	public class ConnectionSigning
	{
		private readonly string _chainId;
		private readonly API _api;
		private readonly Uri _uri;
		
		public ConnectionSigning(int chainId, string apiKey, string baseUrl)
		{
			_chainId = chainId.ToString();
			_api = new API(apiKey, chainId);
			_uri = new Uri(baseUrl);
		}
		
		public async Task SignAndConnect(string walletAddress, ISignerBase signer)
		{
			var nonce = await _api.GetNonce(walletAddress);
			
			var siweMessage = SiweService.CreateMessage(walletAddress, nonce, _chainId, _uri);
			var messageToSign = SiweMessageStringBuilder.BuildMessage(siweMessage);
			var signature = await SignMessage(walletAddress, messageToSign, signer);
			
			await _api.Connect(messageToSign, signature, walletAddress);
		}

		private async Task<string> SignMessage(string walletAddress, string messageToSign, ISignerBase signer)
		{
			var typedData = new TypedData<DomainWithChainIdAndVerifyingContract>
			{
				Domain = new DomainWithChainIdAndVerifyingContract
				{
					ChainId = int.Parse(_chainId),
					VerifyingContract = walletAddress
				},

				Types = new Dictionary<string, MemberDescription[]>
				{
					["EIP712Domain"] = new[]
					{
						new MemberDescription { Name = "chainId", Type = "uint256" },
						new MemberDescription { Name = "verifyingContract", Type = "address" }
					},
					["SafeMessage"] = new[]
					{
						new MemberDescription { Name = "message", Type = "bytes" }
					}
				},
				PrimaryType = "SafeMessage"
			};

			var ethereumMessageSigner = new EthereumMessageSigner();
			var messageBytes = Encoding.UTF8.GetBytes(messageToSign);
			var hashedMessage = ethereumMessageSigner.HashPrefixedMessage(messageBytes);

			var messageTyped = new SafeMessage
			{
				message = hashedMessage.ToHex().EnsureHexPrefix()
			};

			var signature = signer.SignTypedData(messageTyped, typedData);

			return signature;
		}
	}
}