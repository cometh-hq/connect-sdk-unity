using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ComethSDK.Scripts.Enums;
using ComethSDK.Scripts.Interfaces;
using ComethSDK.Scripts.Types.MessageTypes;
using Nethereum.ABI.EIP712;
using Nethereum.Web3;

namespace ComethSDK.Scripts.Tools
{
	public static class Utils
	{
		public static SafeTx CreateSafeTx(string to, string value, string data, int nonce,
			OperationType operationType = 0)
		{
			var safeTx = new SafeTx
			{
				to = to,
				value = value,
				data = data,
				operation = operationType,
				safeTxGas = new BigInteger(0),
				baseGas = new BigInteger(0),
				gasPrice = new BigInteger(0),
				gasToken = Constants.ZERO_ADDRESS,
				refundReceiver = Constants.ZERO_ADDRESS,
				nonce = new BigInteger(nonce).ToString()
			};

			return safeTx;
		}

		public static SafeTx CreateSafeTx(ISafeTransactionDataPartial safeTxData, int nonce)
		{
			var safeTx = new SafeTx
			{
				to = safeTxData.to,
				value = safeTxData.value,
				data = safeTxData.data,
				operation = 0,
				safeTxGas = safeTxData.safeTxGas,
				baseGas = safeTxData.baseGas,
				gasPrice = safeTxData.gasPrice,
				gasToken = Constants.ZERO_ADDRESS,
				refundReceiver = Constants.ZERO_ADDRESS,
				nonce = new BigInteger(nonce).ToString()
			};

			return safeTx;
		}

		public static TypedData<DomainWithChainIdAndVerifyingContract> CreateSafeTxTypedData(string chainId,
			string verifyingContract)
		{
			var typedData = new TypedData<DomainWithChainIdAndVerifyingContract>
			{
				Domain = new DomainWithChainIdAndVerifyingContract
				{
					ChainId = int.Parse(chainId),
					VerifyingContract = verifyingContract
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

			return typedData;
		}

		public static async Task<int> GetNonce(Web3 web3, string contractAddress)
		{
			var addressCode = await web3.Eth.GetCode.SendRequestAsync(contractAddress);
			if (addressCode == "0x") return 0;

			var contract = web3.Eth.GetContract(Constants.SAFE_ABI, contractAddress);
			var function = contract.GetFunction("nonce");
			var result = await function.CallAsync<int>();
			return result;
		}
		
		public static string BytesToString(byte[] privateKey)
		{
			return Encoding.UTF8.GetString(privateKey);
		}
		
		public static byte[] StringToBytes(string privateKey)
		{
			return Encoding.UTF8.GetBytes(privateKey);
		}

		public static bool IsNetworkSupported(string chainId)
		{
			foreach (var network in Constants.Networks.Values)
				if (network.ChainId == chainId)
					return true;

			return false;
		}
	}
}