﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ComethSDK.Scripts.HTTP.RequestBodies;
using ComethSDK.Scripts.HTTP.Responses;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Types;
using ComethSDK.Scripts.Types.MessageTypes;
using Nethereum.ABI.EIP712;
using Nethereum.Siwe.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace ComethSDK.Scripts.HTTP
{
	public class RelayTransactionType
	{
		public RelayTransactionType(SafeTx safeTxData, string signatures, string walletAddress)
		{
			this.safeTxData = safeTxData;
			this.signatures = signatures;
			this.walletAddress = walletAddress;
		}

		public SafeTx safeTxData { get; set; }
		public string signatures { get; set; }
		public string walletAddress { get; set; }
	}

	public class API
	{
		private readonly HttpClient _api;

		public API(string apiKey, int chainId, string baseUrl = Constants.API_URL)
		{
			_api = new HttpClient();
			_api.DefaultRequestHeaders.Add("apikey", apiKey);
			_api.DefaultRequestHeaders.Add("chainId", chainId.ToString());
			_api.BaseAddress = new Uri(baseUrl);
		}

		public async Task<string> RelayTransaction(RelayTransactionType relayTransactionType)
		{
			var safeTxDataTypedWithSignature = new SafeTxWithSignature
			{
				to = relayTransactionType.safeTxData.to,
				value = relayTransactionType.safeTxData.value,
				data = relayTransactionType.safeTxData.data,
				operation = relayTransactionType.safeTxData.operation,
				safeTxGas = relayTransactionType.safeTxData.safeTxGas.ToString(),
				baseGas = relayTransactionType.safeTxData.baseGas.ToString(),
				gasPrice = relayTransactionType.safeTxData.gasPrice.ToString(),
				gasToken = relayTransactionType.safeTxData.gasToken,
				refundReceiver = relayTransactionType.safeTxData.refundReceiver,
				nonce = relayTransactionType.safeTxData.nonce,
				signatures = relayTransactionType.signatures
			};

			var json = JsonConvert.SerializeObject(safeTxDataTypedWithSignature);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var requestUri = "/wallets/" + relayTransactionType.walletAddress + "/relay";
			var response = await _api.PostAsync(requestUri, content);
			var contentReceived = response.Content.ReadAsStringAsync().Result;

			var contentDeserializeObject = JsonConvert.DeserializeObject<RelayTransactionResponse>(contentReceived);

			if (contentDeserializeObject is { success: true }) return contentDeserializeObject.safeTxHash;

			Debug.LogError("Error in RelayTransaction");
			return null;
		}

		public async Task<List<SponsoredAddressResponse.SponsoredAddress>> GetSponsoredAddresses()
		{
			var response = await _api.GetAsync("/sponsored-address");
			var content = response.Content.ReadAsStringAsync().Result;

			var sponsoredAddressesResponse = JsonConvert.DeserializeObject<SponsoredAddressResponse>(content);

			if (sponsoredAddressesResponse is { success: true })
			{
				var sponsoredAddresses = new List<SponsoredAddressResponse.SponsoredAddress>();

				foreach (var sponsoredAddress in sponsoredAddressesResponse.sponsoredAddresses)
					sponsoredAddresses.Add(sponsoredAddress);

				return sponsoredAddresses;
			}

			Debug.Log("Error getting sponsored addresses");
			return null;
		}

		//OwnerAddress is the address of the OEA
		public async Task<string> GetPredictedSafeAddress(string ownerAddress)
		{
			var response = await _api.GetAsync($"/wallets/{ownerAddress}/getWalletAddress");
			var result = response.Content.ReadAsStringAsync().Result;

			var predictedSafeAddressResponse = JsonConvert.DeserializeObject<PredictedSafeAddressResponse>(result);

			if (predictedSafeAddressResponse is { success: true }) return predictedSafeAddressResponse.walletAddress;

			Debug.LogError("Error in GetPredictedSafeAddress");
			return null;
		}

		public async Task<string> ConnectToComethWallet(SiweMessage message, string signature, string walletAddress)
		{
			const string requestUri = "/wallets/connect";
			var siweMessageLowerCase = new SiweMessageLowerCase(message);
			var body = new ConnectToComethWalletBody
			{
				message = siweMessageLowerCase,
				signature = signature,
				walletAddress = walletAddress
			};
			var json = JsonConvert.SerializeObject(body);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await _api.PostAsync(requestUri, content);
			var contentReceived = response.Content.ReadAsStringAsync().Result;
			var contentDeserializeObject =
				JsonConvert.DeserializeObject<ConnectToComethWalletResponse>(contentReceived);

			if (contentDeserializeObject is { success: true }) return contentDeserializeObject.walletAddress;

			Debug.LogError("Error in ConnectToComethWallet");
			return null;
		}

		public async Task<string> ConnectToComethAuth(string jwtToken)
		{
			const string requestUri = "/key-store/connect";
			var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
			request.Headers.Add("token", jwtToken);
			var response = await _api.SendAsync(request);
			var contentReceived = response.Content.ReadAsStringAsync().Result;
			var contentDeserializeObject =
				JsonConvert.DeserializeObject<ConnectToComethAuthResponse>(contentReceived);

			if (contentDeserializeObject is { success: true }) return contentDeserializeObject.address;

			Debug.LogError("Error in ConnectToComethAuth");
			return null;
		}

		public async Task<string> SignTypedDataWithComethAuth(string jwtToken,
			DomainWithChainIdAndVerifyingContractLowerCase domain, IDictionary<string, MemberDescription[]> types,
			IDictionary<string, object> value)
		{
			const string requestUri = "/key-store/signTypedData";
			var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
			request.Headers.Add("token", jwtToken);

			var body = new SignTypedDataWithComethAuthBody
			{
				domain = domain,
				types = types,
				value = value
			};
			var json = JsonConvert.SerializeObject(body);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			request.Content = content;

			var response = await _api.SendAsync(request);
			var contentReceived = response.Content.ReadAsStringAsync().Result;
			var contentDeserializeObject =
				JsonConvert.DeserializeObject<SignTypedDataWithComethAuthResponse>(contentReceived);
			if (contentDeserializeObject is { success: true }) return contentDeserializeObject.signature;

			Debug.LogError("Error in SignTypedDataWithComethAuth");
			return null;
		}

		public async Task<bool> IsValidSignature(string walletAddress, string message, string signature)
		{
			var requestUri = "/wallets/" + walletAddress + "/isValidSignature";
			var isValidSignatureBody = new IsValidSignatureBody
			{
				message = message,
				signature = signature
			};
			var json = JsonConvert.SerializeObject(isValidSignatureBody);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await _api.PostAsync(requestUri, content);
			var contentReceived = response.Content.ReadAsStringAsync().Result;

			if (contentReceived == "{\"success\":true,\"result\":true}") return true;

			Debug.LogError("Error in IsValidSignature");
			return false;
		}

		public async Task<string> GetWalletAddressFromUserID(string jwtToken)
		{
			const string requestUri = "/user/address";
			var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
			request.Headers.Add("token", jwtToken);

			var response = await _api.SendAsync(request);
			var contentReceived = response.Content.ReadAsStringAsync().Result;
			var deserializedResponse =
				JsonConvert.DeserializeObject<GetWalletAddressFromUserIDResponse>(contentReceived);

			if (deserializedResponse.success) return deserializedResponse.walletAddress;

			Debug.LogError("Error in GetWalletAddressFromUserID");
			return null;
		}

		public async Task InitWalletForUserID(string jwtToken, string ownerAddress)
		{
			const string requestUri = "/user/init";
			var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
			request.Headers.Add("token", jwtToken);

			var body = new InitWalletForUserIDBody
			{
				ownerAddress = ownerAddress
			};
			var json = JsonConvert.SerializeObject(body);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			request.Content = content;

			var response = await _api.SendAsync(request);
			var contentReceived = response.Content.ReadAsStringAsync().Result;

			Debug.Log(contentReceived);
		}

		public async Task<string> InitWallet(string ownerAddress)
		{
			const string requestUri = "/wallets/init";
			var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

			var body = new InitWalletForUserIDBody
			{
				ownerAddress = ownerAddress
			};
			var json = JsonConvert.SerializeObject(body);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			request.Content = content;

			var response = await _api.SendAsync(request);
			var contentReceived = response.Content.ReadAsStringAsync().Result;

			var initWalletResponse = JsonConvert.DeserializeObject<InitWalletResponse>(contentReceived);

			if (initWalletResponse is { success: true }) return initWalletResponse.walletAddress;

			Debug.LogError("Error in InitWallet");
			return null;
		}

		public async Task<string> GetNonce(string walletAddress)
		{
			var response = await _api.GetAsync($"/wallets/{walletAddress}/connection-nonce");
			var result = response.Content.ReadAsStringAsync().Result;

			var nonceResponse = JsonConvert.DeserializeObject<NonceResponse>(result);

			return nonceResponse is { success: true } ? nonceResponse.userNonce.connectionNonce : null;
		}

		public async Task<string> GetWalletAddress(string ownerAddress)
		{
			var response = await _api.GetAsync($"/wallets/{ownerAddress}/wallet-address");
			var result = response.Content.ReadAsStringAsync().Result;

			var predictedSafeAddressResponse = JsonConvert.DeserializeObject<PredictedSafeAddressResponse>(result);

			if (predictedSafeAddressResponse is { success: true }) return predictedSafeAddressResponse.walletAddress;

			Debug.LogError("Error in GetWalletAddress");
			return null;
		}

		public async Task Connect(SiweMessage messageToSign, string signature, string walletAddress)
		{
			const string requestUri = "/wallets/connect";
			var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

			var body = new ConnectBody
			{
				message = new SiweMessageLowerCase(messageToSign),
				signature = signature,
				walletAddress = walletAddress
			};
			var json = JsonConvert.SerializeObject(body);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			request.Content = content;

			var response = await _api.SendAsync(request);
			var contentReceived = response.Content.ReadAsStringAsync().Result;

			Debug.Log(contentReceived);
		}

		public async Task<WalletInfos> GetWalletInfos(string walletAddress)
		{
			var response = await _api.GetAsync($"/wallets/{walletAddress}/wallet-infos");
			var result = response.Content.ReadAsStringAsync().Result;

			var getWalletInfosResponse = JsonConvert.DeserializeObject<GetWalletInfosResponse>(result);

			if (getWalletInfosResponse is { success: true }) return getWalletInfosResponse.walletInfos;

			Debug.LogError("Error in GetWalletInfos");
			return null;
		}

		public async Task<NewSignerRequestBody[]> GetNewSignerRequests(string walletAddress)
		{
			var response = await _api.GetAsync($"/new-signer-request/{walletAddress}");
			var result = response.Content.ReadAsStringAsync().Result;

			var getNewSignerRequestsResponse = JsonConvert.DeserializeObject<GetNewSignerRequestsResponse>(result);

			if (getNewSignerRequestsResponse is { success: true }) return getNewSignerRequestsResponse.signerRequests;

			Debug.LogError("Error in GetNewSignerRequests");
			return null;
		}

		public async Task<ProjectParams> GetProjectParams()
		{
			var response = await _api.GetAsync("/project/params");
			var result = response.Content.ReadAsStringAsync().Result;

			var getNewSignerRequestsResponse = JsonConvert.DeserializeObject<GetProjectParamsResponse>(result);

			if (getNewSignerRequestsResponse is { success: true }) return getNewSignerRequestsResponse.projectParams;

			Debug.LogError("Error in GetProjectParams");
			return null;
		}
	}
}