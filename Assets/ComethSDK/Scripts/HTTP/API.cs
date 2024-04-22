using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ComethSDK.Scripts.HTTP.RequestBodies;
using ComethSDK.Scripts.HTTP.Responses;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Types;
using ComethSDK.Scripts.Types.MessageTypes;
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

		public SafeTx safeTxData { get; }
		public string signatures { get; }
		public string walletAddress { get; }
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
				{
					var newSponsoredAddress = sponsoredAddress;
					newSponsoredAddress.targetAddress = sponsoredAddress.targetAddress.ToLower();
					sponsoredAddresses.Add(newSponsoredAddress);
				}

				return sponsoredAddresses;
			}

			Debug.Log("Error getting sponsored addresses");
			return null;
		}

		public async Task InitWallet(string ownerAddress)
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

			if (initWalletResponse is { success: true }) return;

			Debug.LogError("Error in InitWallet");
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