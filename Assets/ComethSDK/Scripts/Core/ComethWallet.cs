using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ComethSDK.Scripts.Adapters.Interfaces;
using ComethSDK.Scripts.Enums;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.HTTP.Responses;
using ComethSDK.Scripts.Interfaces;
using ComethSDK.Scripts.Services;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Types;
using ComethSDK.Scripts.Types.MessageTypes;
using JetBrains.Annotations;
using Nethereum.ABI.EIP712;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Siwe.Core;
using Nethereum.Web3;
using UnityEngine;
using EventHandler = ComethSDK.Scripts.Tools.EventHandler;

namespace ComethSDK.Scripts.Core
{
	public class ComethWallet
	{
		private readonly API _api;
		private readonly IAuthAdaptor _authAdaptor;
		private readonly string _chainId;
		private readonly Uri _uri = new("https://api.connect.cometh.io");
		private readonly BigInteger BASE_GAS = Constants.DEFAULT_BASE_GAS;
		private bool _connected;
		private EventHandler _eventHandler;
		private Constants.Network _network;
		private ProjectParams _projectParams;
		private string _rpcUrl;

		private List<SponsoredAddressResponse.SponsoredAddress> _sponsoredAddresses = new();
		private string _walletAddress;
		private Web3 _web3;

		public ComethWallet(IAuthAdaptor authAdaptor, string apiKey, string baseUrl = "", string rpcUrl = "")
		{
			if (!Utils.IsNetworkSupported(authAdaptor.ChainId)) throw new Exception("This network is not supported");
			_chainId = authAdaptor.ChainId;
			_api = string.IsNullOrEmpty(baseUrl)
				? new API(apiKey, int.Parse(_chainId))
				: new API(apiKey, int.Parse(_chainId), baseUrl);
			_rpcUrl = string.IsNullOrEmpty(rpcUrl)
				? Constants.GetNetworkByChainID(_chainId).RPCUrl
				: rpcUrl;
			__web3 = new Web3(_rpcUrl)
			_authAdaptor = authAdaptor;
		}

		public async Task Connect([CanBeNull] string burnerAddress = "")
		{
			if (_authAdaptor == null) throw new Exception("No auth adaptor found");

			await _authAdaptor.Connect(burnerAddress);

			_projectParams = await _api.GetProjectParams();
			var account = _authAdaptor.GetAccount();
			var predictedWalletAddress = await _api.GetWalletAddress(account);
			_walletAddress = predictedWalletAddress ?? throw new Exception("Error while getting wallet address");

			var nonce = await _api.GetNonce(predictedWalletAddress);
			if (nonce == null) throw new Exception("Error while getting nonce");

			var message = SiweService.CreateMessage(predictedWalletAddress, nonce);
			var messageToSign = SiweMessageStringBuilder.BuildMessage(message);
			var signatureSiwe = await SignMessage(messageToSign);

			//SAFE ADDRESS
			var walletAddress = await _api.ConnectToComethWallet(
				message,
				signatureSiwe,
				predictedWalletAddress
			);
			if (walletAddress == null) throw new Exception("Error while connecting to Cometh Wallet");

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

			var userInfos = new UserInfos
			{
				ownerAddress = _authAdaptor.GetAccount(),
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
			CheckIsLoggedIn();

			var tx = await SafeService.PrepareAddOwnerTx(GetAddress(), newOwner, _rpcUrl);

			var safeTxHash = await SendTransaction(tx);

			return safeTxHash;
		}

		public async Task<string> RemoveOwner(string owner)
		{
			CheckIsLoggedIn();

			var tx = await SafeService.PrepareRemoveOwnerTx(GetAddress(), owner, _rpcUrl);

			//TODO: remove the local storage of the private key

			var safeTxHash = await SendTransaction(tx);

			return safeTxHash;
		}

		public async Task<List<string>> GetOwners()
		{
			return await SafeService.GetOwners(_walletAddress, _rpcUrl);
		}

		public void CancelWaitingForEvent()
		{
			_eventHandler.CancelWait();
		}

		/**
		   * Signing Message Section
		   */
		public async Task<string> SignMessage(string message)
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

			var signature = await SignTypedData(messageTyped, typedData);

			return signature;
		}

		/**
		 * Transaction Section
		 */
		//TODO: change return type to SendTransactionResponse
		public async Task<string> SendTransaction(string to, string value, string data)
		{
			return await SendTransaction(new MetaTransactionData
			{
				to = to,
				value = value,
				data = data
			});
		}

		public async Task<string> SendTransaction(MetaTransactionData safeTxData)
		{
			CheckIsLoggedIn();

			var safeTxDataArray = new IMetaTransactionData[]
			{
				safeTxData
			};

			var nonce = await Utils.GetNonce(_web3, _walletAddress);
			var typedData = Utils.CreateSafeTxTypedData(_chainId, _walletAddress);
			var safeTx = Utils.CreateSafeTx(safeTxDataArray[0].to, safeTxDataArray[0].value, safeTxDataArray[0].data,
				nonce);

			if (!IsSponsoredTransaction(safeTxDataArray))
			{
				safeTx = await GasService.SetTransactionGasWithSimulate(safeTx, _walletAddress, _projectParams.MultiSendContractAddress,
					_projectParams.SafeSingletonAddress,
					_projectParams.SafeTxAccessorAddress
					, _rpcUrl);
				await GasService.VerifyHasEnoughBalance(_walletAddress, safeTxDataArray[0].to, safeTxDataArray[0].value,
					safeTxDataArray[0].data, nonce, _rpcUrl);
			}

			var txSignature = await SignTypedData(safeTx, typedData);
			Debug.Log("Sending Transaction");
			return await _api.RelayTransaction(new RelayTransactionType(
				safeTx, txSignature, _walletAddress)
			);
		}

		//TODO: change return type to SendTransactionResponse
		public async Task<string> SendBatchTransactions(IMetaTransactionData[] safeTxData)
		{
			CheckIsLoggedIn();
			if (safeTxData.Length == 0) throw new Exception("Empty array provided, no transaction to send");
			if (_projectParams == null) throw new Exception("Project params not found");

			var nonce = await Utils.GetNonce(_web3, _walletAddress);
			var multiSendData = MultiSend
				.EncodeMultiSendArray(safeTxData, _rpcUrl, _projectParams.MultiSendContractAddress)
				.data;
			var safeTx = Utils.CreateSafeTx(_projectParams.MultiSendContractAddress, "0", multiSendData, nonce,
				OperationType.DELEGATE_CALL);
			var dataType = Utils.CreateSafeTxTypedData(_chainId, _walletAddress);

			if (!IsSponsoredTransaction(safeTxData))
			{
				var safeTxGasString = await GasService.EstimateSafeTxGasWithSimulate(_walletAddress, safeTxData,
					_projectParams.MultiSendContractAddress,
					_projectParams.SafeSingletonAddress,
					_projectParams.SafeTxAccessorAddress,
					_rpcUrl);

				var gasEstimates = new GasEstimates
				{
					baseGas = BASE_GAS,
					gasPrice = await GasService.GetGasPrice(_rpcUrl),
					safeTxGas = BigInteger.Parse(safeTxGasString)
				};

				var txValue = SafeService.GetTransactionsTotalValue(safeTxData);
				await GasService.VerifyHasEnoughBalance(_walletAddress, gasEstimates, txValue, _rpcUrl);

				safeTx.safeTxGas += gasEstimates.safeTxGas;
				safeTx.baseGas = gasEstimates.baseGas;
				safeTx.gasPrice += gasEstimates.gasPrice;
			}

			return await SignAndSendTransaction(safeTx, dataType);
		}

		public async Task<string> SignAndSendTransaction(SafeTx safeTx,
			TypedData<DomainWithChainIdAndVerifyingContract> typedData)
		{
			var txSignature = await SignTypedData(safeTx, typedData);

			return await _api.RelayTransaction(new RelayTransactionType(
				safeTx, txSignature, _walletAddress)
			);
		}

		/**
		 * Private Methods
		 */

		private async Task<string> SignTypedData<T, TDomain>(T message, TypedData<TDomain> typedData)
		{
			var signer = _authAdaptor.GetSigner();
			return signer.SignTypedData(message, typedData);
		}

		private bool IsSponsoredTransaction(IMetaTransactionData[] safeTxDataArray)
		{
			foreach (var safeTxData in safeTxDataArray)
			{
				var functionSelector = SafeService.GetFunctionSelector(safeTxData);
				var sponsoredAddress = ToSponsoredAddress(safeTxData.to);

				if (!sponsoredAddress && functionSelector != Constants.ADD_OWNER_FUNCTION_SELECTOR) return false;
			}

			return true;
		}

		private bool ToSponsoredAddress(string to)
		{
			//if index >= 0 then address is sponsored
			var index = _sponsoredAddresses.FindIndex(
				sponsoredAddress => sponsoredAddress.targetAddress == to.ToLower());
			return index >= 0;
		}

		private void CheckIsLoggedIn()
		{
			if (!_connected)
			{
				Debug.Log("Please Login First");
				throw new InvalidOperationException("Connection not established. Please log in first.");
			}
		}
	}
}