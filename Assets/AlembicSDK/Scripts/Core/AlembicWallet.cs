using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AlembicSDK.Scripts.HTTP;
using AlembicSDK.Scripts.HTTP.Responses;
using AlembicSDK.Scripts.Interfaces;
using AlembicSDK.Scripts.Tools;
using AlembicSDK.Scripts.Types;
using AlembicSDK.Scripts.Types.MessageTypes;
using Nethereum.ABI.EIP712;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.Signer;
using Nethereum.Siwe.Core;
using Nethereum.Web3;
using UnityEngine;
using EventHandler = AlembicSDK.Scripts.Tools.EventHandler;

namespace AlembicSDK.Scripts.Core
{
	public class AlembicWallet
	{
		private readonly API _api;
		private readonly IAuthAdaptor _authAdaptor;
		private readonly string _chainId;
		private readonly Uri _uri = new("https://api.alembic.finance");
		private bool _connected;
		private EventHandler _eventHandler;
		private Constants.Network _network;

		private List<SponsoredAddressResponse.SponsoredAddress> _sponsoredAddresses = new();
		private string _walletAddress;
		private Web3 _web3;

		private Web3Auth _web3Auth;
		private readonly BigInteger BASE_GAS;
		private readonly double REWARD_PERCENTILE;

		public AlembicWallet(IAuthAdaptor authAdaptor, string apiKey)
		{
			_authAdaptor = authAdaptor;
			_chainId = authAdaptor.ChainId;
			_api = new API(apiKey, int.Parse(_chainId));
			BASE_GAS = Constants.DEFAULT_BASE_GAS;
			REWARD_PERCENTILE = Constants.DEFAULT_REWARD_PERCENTILE;
		}

		public async Task Connect()
		{
			if (_authAdaptor == null) throw new Exception("No EOA adapter found");

			if (!Constants.IsNetworkSupported(_chainId)) throw new Exception("This network is not supported");

			_web3 = new Web3(Constants.GetNetworkByChainID(_chainId).RPCUrl);

			await _authAdaptor.Connect();

			var account = _authAdaptor.GetAccount();
			var ownerAddress = account.Address;
			var predictedWalletAddress = await _api.GetPredictedSafeAddress(ownerAddress);
			_walletAddress = predictedWalletAddress ?? throw new Exception("Error while getting wallet address");

			var nonce = await _api.GetNonce(predictedWalletAddress);
			if (nonce == null) throw new Exception("Error while getting nonce");

			var message = CreateMessage(predictedWalletAddress, nonce);
			var messageToSign = SiweMessageStringBuilder.BuildMessage(message);
			var signatureSiwe = SignMessage(messageToSign);

			//SAFE ADDRESS
			var walletAddress = await _api.ConnectToAlembicWallet(
				message,
				signatureSiwe,
				predictedWalletAddress
			);
			if (walletAddress == null) throw new Exception("Error while connecting to Alembic Wallet");

			_sponsoredAddresses = await _api.GetSponsoredAddresses();
			if (_sponsoredAddresses == null) throw new Exception("Error while getting sponsored addresses");

			_connected = true;
			_eventHandler = new EventHandler(_web3, _walletAddress);
		}

		public async Task<TransactionReceipt> Wait(string safeTxHash)
		{
			return await _eventHandler.Wait(safeTxHash);
		}

		public Contract GetContract(string abi, string address)
		{
			return _web3.Eth.GetContract(abi, address);
		}

		public bool GetConnected()
		{
			return _connected;
		}

		public UserInfos GetUserInfos()
		{
			if (_authAdaptor == null) throw new Exception("Cannot provide user infos");

			var userInfo = _authAdaptor.GetUserInfos();
			var userInfos = new UserInfos
			{
				email = userInfo.email,
				name = userInfo.name,
				profileImage = userInfo.profileImage,
				aggregateVerifier = userInfo.aggregateVerifier,
				verifier = userInfo.verifier,
				verifierId = userInfo.verifierId,
				typeOfLogin = userInfo.typeOfLogin,
				dappShare = userInfo.dappShare,
				idToken = userInfo.idToken,
				oAuthIdToken = userInfo.oAuthIdToken,
				oAuthAccessToken = userInfo.oAuthAccessToken,
				ownerAddress = _authAdaptor.GetAccount().Address,
				walletAddress = _walletAddress
			};
			return userInfos;
		}

		public string GetAddress()
		{
			return _walletAddress;
		}

		public async Task<BigInteger> GetBalance(string address)
		{
			return await _web3.Eth.GetBalance.SendRequestAsync(address);
		}

		public async Task Logout()
		{
			if (_authAdaptor == null) throw new Exception("No EOA adapter found");
			await _authAdaptor.Logout();
			_connected = false;
		}

		public async Task<string> AddOwner(string newOwner)
		{
			if (!_connected)
			{
				Debug.Log("Please Login First");
				return "";
			}

			var to = _walletAddress;
			const string value = "0";

			var contract = _web3.Eth.GetContract(Constants.SAFE_ABI, _walletAddress);
			var addOwnerWithThresholdFunction = contract.GetFunction("addOwnerWithThreshold");
			var data = addOwnerWithThresholdFunction.GetData(newOwner, 1);

			var safeTxHash = await SendTransaction(to, value, data);

			return safeTxHash;
		}

		public void CancelWaitingForEvent()
		{
			_eventHandler.CancelWait();
		}

		/**
		   * Signing Message Section
		   */
		public string SignMessage(string message)
		{
			var typedData = new TypedData<DomainWithChainIdAndVerifyingContract>
			{
				Domain = new DomainWithChainIdAndVerifyingContract
				{
					ChainId = int.Parse(_chainId),
					VerifyingContract = _walletAddress
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
			var messageBytes = Encoding.UTF8.GetBytes(message);
			var hashedMessage = ethereumMessageSigner.HashPrefixedMessage(messageBytes);

			var messageTyped = new SafeMessage
			{
				message = hashedMessage.ToHex().EnsureHexPrefix()
			};

			var signer = _authAdaptor.GetSigner();
			var signature = signer.SignTypedData(messageTyped, typedData);

			return signature;
		}

		/**
		 * Transaction Section
		 */
		public async Task<string> SendTransaction(string to, string value, string data)
		{
			if (!_connected)
			{
				Debug.Log("Please Login First");
				return "";
			}

			var nonce = await Tools.Utils.GetNonce(_web3, _walletAddress);
			var typedData = Tools.Utils.CreateSafeTxTypedData(_chainId, _walletAddress);
			var safeTx = Tools.Utils.CreateSafeTx(to, value, data, nonce);

			if (!ToSponsoredAddress(safeTx.to)) safeTx = await SetTransactionGas(safeTx);

			var txSignature = Tools.Utils.SignSafeMessage(_authAdaptor.GetSigner(), safeTx, typedData);

			Debug.Log("Sending Transaction");
			return await _api.RelayTransaction(new RelayTransactionType(
				safeTx, txSignature, _walletAddress)
			);
		}

		public async Task<GasEstimates> EstimateTransactionGas(ISafeTransactionDataPartial safeTxData)
		{
			var safeTxGas = safeTxData.safeTxGas;
			safeTxGas += await CalculateSafeTxGas(safeTxData.data, safeTxData.to);

			var gasPrice = safeTxData.gasPrice;
			gasPrice += await CalculateGasPrice();

			return new GasEstimates { safeTxGas = safeTxGas, baseGas = BASE_GAS, gasPrice = gasPrice };
		}

		public async Task<BigInteger> CalculateMaxFees(string to, string value, string data, int nonce)
		{
			var safeTx = Tools.Utils.CreateSafeTx(to, value, data, nonce);
			safeTx = await SetTransactionGas(safeTx);
			var totalGasCost = (safeTx.safeTxGas + safeTx.baseGas) * safeTx.gasPrice;
			return totalGasCost + BigInteger.Parse(value);
		}

		/**
		 * Private Methods
		 */
		private SiweMessage CreateMessage(string address, string nonce)
		{
			var domain = _uri.Host;
			var origin = _uri.Scheme + "://" + _uri.Host;
			const string statement = "Sign in with Ethereum to Alembic";

			var message = new SiweMessage
			{
				Domain = domain,
				Address = address,
				Statement = statement,
				Uri = origin,
				Version = "1",
				ChainId = _chainId,
				Nonce = nonce
			};

			message.SetIssuedAtNow();

			return message;
		}

		private bool ToSponsoredAddress(string to)
		{
			//if index >= 0 then address is sponsored
			var index = _sponsoredAddresses.FindIndex(sponsoredAddress => sponsoredAddress.TargetAddress == to);
			return index >= 0;
		}

		private async Task<BigInteger> CalculateSafeTxGas(string data, string to)
		{
			var ethEstimateGas = new EthEstimateGas(_web3.Client);

			var transactionInput = new CallInput
			{
				Data = data,
				To = to,
				From = _walletAddress
			};
			return await ethEstimateGas.SendRequestAsync(transactionInput);
		}

		private async Task<BigInteger> CalculateGasPrice()
		{
			var ethFeeHistory = await _web3.Eth.FeeHistory.SendRequestAsync(
				new HexBigInteger(1),
				new BlockParameter(),
				new[] { REWARD_PERCENTILE });

			var reward = ethFeeHistory.Reward[0][0].Value;
			var baseFee = ethFeeHistory.BaseFeePerGas[0].Value;

			return reward + baseFee + (reward + baseFee) / 10;
		}

		private async Task<SafeTx> SetTransactionGas(SafeTx safeTxDataTyped)
		{
			var gasEstimates = await EstimateTransactionGas(safeTxDataTyped);
			safeTxDataTyped.safeTxGas = gasEstimates.safeTxGas;
			safeTxDataTyped.baseGas = gasEstimates.baseGas;
			safeTxDataTyped.gasPrice = gasEstimates.gasPrice;

			return safeTxDataTyped;
		}
	}
}