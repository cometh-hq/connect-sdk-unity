using System.Collections.Generic;
using System.Threading.Tasks;
using AlembicSDK.Scripts.Core;
using AlembicSDK.Scripts.Tools;
using AlembicSDK.Scripts.Types.MessageTypes;
using CandyCoded.env;
using Nethereum.ABI.EIP712;
using NUnit.Framework;
using UnityEngine;

namespace AlembicSDK.Tests
{
	public class TestUtils
	{
		private const string CHAINID = "80001";

		private const string
			PRIVATEKEY =
				"0x08aa89107bccf655b326aae1ec53fffed17c726d4d2df295510cacd215a046b8"; //USING HARDCODED PRIVATE KEY

		private const string WALLETADDRESS = "0xE0f3CcBf9d66272aE8B67891E4Edc3eff67C6bAE";
		private string APIKEY = "";

		[Test]
		public async Task CreateSafeTxTypedData()
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

			var walletAdapter = new FixedSignerAdaptor(CHAINID, PRIVATEKEY);
			await walletAdapter.Connect();

			var wallet = new AlembicWallet(walletAdapter, APIKEY);
			await wallet.Connect();

			var walletAddress = wallet.GetAddress();
			Assert.AreEqual(WALLETADDRESS, walletAddress);

			var nonce = 22;
			var to = "0x510c522ebCC6Eb376839E0CFf5D57bb2F422EB8b";
			var value = "0";
			var data = "0x";

			var typedData = Scripts.Tools.Utils.CreateSafeTxTypedData(CHAINID, walletAddress);
			var expectedTypedData = new TypedData<DomainWithChainIdAndVerifyingContract>
			{
				Domain = new DomainWithChainIdAndVerifyingContract
				{
					ChainId = int.Parse(CHAINID),
					VerifyingContract = walletAddress
				},

				Types = new Dictionary<string, MemberDescription[]>
				{
					["EIP712Domain"] = new[]
					{
						new MemberDescription { Name = "chainId", Type = "uint256" },
						new MemberDescription { Name = "verifyingContract", Type = "address" }
					},
					["SafeTx"] = new[]
					{
						new MemberDescription { Name = "to", Type = "address" },
						new MemberDescription { Name = "value", Type = "uint256" },
						new MemberDescription { Name = "data", Type = "bytes" },
						new MemberDescription { Name = "operation", Type = "uint8" },
						new MemberDescription { Name = "safeTxGas", Type = "uint256" },
						new MemberDescription { Name = "baseGas", Type = "uint256" },
						new MemberDescription { Name = "gasPrice", Type = "uint256" },
						new MemberDescription { Name = "gasToken", Type = "address" },
						new MemberDescription { Name = "refundReceiver", Type = "address" },
						new MemberDescription { Name = "nonce", Type = "uint256" }
					}
				},
				PrimaryType = "SafeTx"
			};
			Assert.AreEqual(expectedTypedData.Domain.ChainId, typedData.Domain.ChainId);
			Assert.AreEqual(expectedTypedData.Domain.VerifyingContract, typedData.Domain.VerifyingContract);
			Assert.AreEqual(expectedTypedData.PrimaryType, typedData.PrimaryType);
			Assert.AreEqual(expectedTypedData.Types["EIP712Domain"][0].Name, typedData.Types["EIP712Domain"][0].Name);
			Assert.AreEqual(expectedTypedData.Types["EIP712Domain"][0].Type, typedData.Types["EIP712Domain"][0].Type);
			Assert.AreEqual(expectedTypedData.Types["EIP712Domain"][1].Name, typedData.Types["EIP712Domain"][1].Name);
			Assert.AreEqual(expectedTypedData.Types["EIP712Domain"][1].Type, typedData.Types["EIP712Domain"][1].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][0].Name, typedData.Types["SafeTx"][0].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][0].Type, typedData.Types["SafeTx"][0].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][1].Name, typedData.Types["SafeTx"][1].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][1].Type, typedData.Types["SafeTx"][1].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][2].Name, typedData.Types["SafeTx"][2].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][2].Type, typedData.Types["SafeTx"][2].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][3].Name, typedData.Types["SafeTx"][3].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][3].Type, typedData.Types["SafeTx"][3].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][4].Name, typedData.Types["SafeTx"][4].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][4].Type, typedData.Types["SafeTx"][4].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][5].Name, typedData.Types["SafeTx"][5].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][5].Type, typedData.Types["SafeTx"][5].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][6].Name, typedData.Types["SafeTx"][6].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][6].Type, typedData.Types["SafeTx"][6].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][7].Name, typedData.Types["SafeTx"][7].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][7].Type, typedData.Types["SafeTx"][7].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][8].Name, typedData.Types["SafeTx"][8].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][8].Type, typedData.Types["SafeTx"][8].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][9].Name, typedData.Types["SafeTx"][9].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][9].Type, typedData.Types["SafeTx"][9].Type);
		}

		[Test]
		public async Task CreateSafeTx()
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

			var walletAdapter = new FixedSignerAdaptor(CHAINID, PRIVATEKEY);
			await walletAdapter.Connect();

			var wallet = new AlembicWallet(walletAdapter, APIKEY);
			await wallet.Connect();

			var walletAddress = wallet.GetAddress();
			Assert.AreEqual(WALLETADDRESS, walletAddress);

			var nonce = 22;
			var to = "0x510c522ebCC6Eb376839E0CFf5D57bb2F422EB8b";
			var value = "0";
			var data = "0x";

			var safeTx = Scripts.Tools.Utils.CreateSafeTx(to, value, data, nonce);
			var expectedSafeTx = new SafeTx
			{
				baseGas = 0,
				data = data,
				gasPrice = 0,
				gasToken = Constants.ZERO_ADDRESS,
				nonce = nonce.ToString(),
				operation = 0,
				refundReceiver = Constants.ZERO_ADDRESS,
				safeTxGas = 0,
				to = to,
				value = value
			};

			Assert.AreEqual(expectedSafeTx.data, safeTx.data);
			Assert.AreEqual(expectedSafeTx.nonce, safeTx.nonce);
			Assert.AreEqual(expectedSafeTx.operation, safeTx.operation);
			Assert.AreEqual(expectedSafeTx.refundReceiver, safeTx.refundReceiver);
			Assert.AreEqual(expectedSafeTx.safeTxGas, safeTx.safeTxGas);
			Assert.AreEqual(expectedSafeTx.to, safeTx.to);
			Assert.AreEqual(expectedSafeTx.value, safeTx.value);
			Assert.AreEqual(expectedSafeTx.baseGas, safeTx.baseGas);
			Assert.AreEqual(expectedSafeTx.gasPrice, safeTx.gasPrice);
			Assert.AreEqual(expectedSafeTx.gasToken, safeTx.gasToken);
		}

		[Test]
		public async Task SignSafeMessage()
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

			var walletAdapter = new FixedSignerAdaptor(CHAINID, PRIVATEKEY);
			await walletAdapter.Connect();

			var wallet = new AlembicWallet(walletAdapter, APIKEY);
			await wallet.Connect();

			var walletAddress = wallet.GetAddress();
			Assert.AreEqual(WALLETADDRESS, walletAddress);

			var nonce = 22;
			var to = "0x510c522ebCC6Eb376839E0CFf5D57bb2F422EB8b";
			var value = "0";
			var data = "0x";

			var typedData = Scripts.Tools.Utils.CreateSafeTxTypedData(CHAINID, walletAddress);
			var expectedTypedData = new TypedData<DomainWithChainIdAndVerifyingContract>
			{
				Domain = new DomainWithChainIdAndVerifyingContract
				{
					ChainId = int.Parse(CHAINID),
					VerifyingContract = walletAddress
				},

				Types = new Dictionary<string, MemberDescription[]>
				{
					["EIP712Domain"] = new[]
					{
						new MemberDescription { Name = "chainId", Type = "uint256" },
						new MemberDescription { Name = "verifyingContract", Type = "address" }
					},
					["SafeTx"] = new[]
					{
						new MemberDescription { Name = "to", Type = "address" },
						new MemberDescription { Name = "value", Type = "uint256" },
						new MemberDescription { Name = "data", Type = "bytes" },
						new MemberDescription { Name = "operation", Type = "uint8" },
						new MemberDescription { Name = "safeTxGas", Type = "uint256" },
						new MemberDescription { Name = "baseGas", Type = "uint256" },
						new MemberDescription { Name = "gasPrice", Type = "uint256" },
						new MemberDescription { Name = "gasToken", Type = "address" },
						new MemberDescription { Name = "refundReceiver", Type = "address" },
						new MemberDescription { Name = "nonce", Type = "uint256" }
					}
				},
				PrimaryType = "SafeTx"
			};
			Assert.AreEqual(expectedTypedData.Domain.ChainId, typedData.Domain.ChainId);
			Assert.AreEqual(expectedTypedData.Domain.VerifyingContract, typedData.Domain.VerifyingContract);
			Assert.AreEqual(expectedTypedData.PrimaryType, typedData.PrimaryType);
			Assert.AreEqual(expectedTypedData.Types["EIP712Domain"][0].Name, typedData.Types["EIP712Domain"][0].Name);
			Assert.AreEqual(expectedTypedData.Types["EIP712Domain"][0].Type, typedData.Types["EIP712Domain"][0].Type);
			Assert.AreEqual(expectedTypedData.Types["EIP712Domain"][1].Name, typedData.Types["EIP712Domain"][1].Name);
			Assert.AreEqual(expectedTypedData.Types["EIP712Domain"][1].Type, typedData.Types["EIP712Domain"][1].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][0].Name, typedData.Types["SafeTx"][0].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][0].Type, typedData.Types["SafeTx"][0].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][1].Name, typedData.Types["SafeTx"][1].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][1].Type, typedData.Types["SafeTx"][1].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][2].Name, typedData.Types["SafeTx"][2].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][2].Type, typedData.Types["SafeTx"][2].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][3].Name, typedData.Types["SafeTx"][3].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][3].Type, typedData.Types["SafeTx"][3].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][4].Name, typedData.Types["SafeTx"][4].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][4].Type, typedData.Types["SafeTx"][4].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][5].Name, typedData.Types["SafeTx"][5].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][5].Type, typedData.Types["SafeTx"][5].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][6].Name, typedData.Types["SafeTx"][6].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][6].Type, typedData.Types["SafeTx"][6].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][7].Name, typedData.Types["SafeTx"][7].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][7].Type, typedData.Types["SafeTx"][7].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][8].Name, typedData.Types["SafeTx"][8].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][8].Type, typedData.Types["SafeTx"][8].Type);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][9].Name, typedData.Types["SafeTx"][9].Name);
			Assert.AreEqual(expectedTypedData.Types["SafeTx"][9].Type, typedData.Types["SafeTx"][9].Type);


			var safeTx = Scripts.Tools.Utils.CreateSafeTx(to, value, data, nonce);
			var expectedSafeTx = new SafeTx
			{
				baseGas = 0,
				data = data,
				gasPrice = 0,
				gasToken = Constants.ZERO_ADDRESS,
				nonce = nonce.ToString(),
				operation = 0,
				refundReceiver = Constants.ZERO_ADDRESS,
				safeTxGas = 0,
				to = to,
				value = value
			};

			Assert.AreEqual(expectedSafeTx.data, safeTx.data);
			Assert.AreEqual(expectedSafeTx.nonce, safeTx.nonce);
			Assert.AreEqual(expectedSafeTx.operation, safeTx.operation);
			Assert.AreEqual(expectedSafeTx.refundReceiver, safeTx.refundReceiver);
			Assert.AreEqual(expectedSafeTx.safeTxGas, safeTx.safeTxGas);
			Assert.AreEqual(expectedSafeTx.to, safeTx.to);
			Assert.AreEqual(expectedSafeTx.value, safeTx.value);
			Assert.AreEqual(expectedSafeTx.baseGas, safeTx.baseGas);
			Assert.AreEqual(expectedSafeTx.gasPrice, safeTx.gasPrice);
			Assert.AreEqual(expectedSafeTx.gasToken, safeTx.gasToken);

			var signer = walletAdapter.GetSigner();
			var txSignature = Scripts.Tools.Utils.SignSafeMessage(signer, safeTx, typedData);
			var expectedTxSignature =
				"0x28a0f3a9d638cfb4b098278cc90cce4d64e7784d8d8d2e67d5a6c87fe5cbf1125598607d1a1502a7efdb3b2310373f1fc72d46a7eb2eff454c04fdcfc3d0d0f41c";
			Assert.AreEqual(expectedTxSignature, txSignature);
		}
	}
}