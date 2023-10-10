using System;
using System.Threading.Tasks;
using ComethSDK.Scripts.Adapters.Interfaces;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Services;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Tools.Signers;
using ComethSDK.Scripts.Tools.Signers.Interfaces;
using ComethSDK.Scripts.Types;
using UnityEngine;

namespace ComethSDK.Scripts.Adapters
{
	public class ConnectWithJwtAdaptor : MonoBehaviour, IAuthAdaptor
	{
		[SerializeField] private int chainId;
		[SerializeField] private string jwtToken;
		[SerializeField] private string apiKey;

		[SerializeField] private string userName;
		[SerializeField] private string rpcUrl; //TODO: implement this
		[SerializeField] private string baseUrl; //TODO: implement this

		private string _account;
		private API _api;
		private Signer _signer;

		private void Awake()
		{
			if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(apiKey) || chainId == 0 ||
			    string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(rpcUrl) || string.IsNullOrEmpty(baseUrl))
				Debug.LogError("Serialized fields empty");
			
			if (!Utils.IsNetworkSupported(chainId.ToString())) throw new Exception("This network is not supported");
			ChainId = chainId.ToString();
			
			_api = new API(apiKey, chainId);
		}

		public string ChainId { get; private set; }

		public async Task Connect(string burnerAddress)
		{
			var walletAddress = await _api.GetWalletAddressFromUserID(jwtToken);
			var userID = TokenService.DecodeTokenAndGetUserID(jwtToken);
			_signer = string.IsNullOrEmpty(walletAddress) ? await BurnerWalletService.GetSignerForUserId(userID, walletAddress, _api,
				Constants.GetNetworkByChainID(ChainId).RPCUrl) : await BurnerWalletService.CreateSignerForUserId(jwtToken, userID, walletAddress, _api,
				Constants.GetNetworkByChainID(ChainId).RPCUrl);
			Debug.Log("here");
		}

		public Task Logout()
		{
			if (_signer == null) throw new Exception("No signer instance found");

			PlayerPrefs.DeleteKey("privateKey");
			_account = null;
			_signer = null;
			_api = null;
			return Task.CompletedTask;
		}

		public string GetAccount()
		{
			if (_signer == null) throw new Exception("No signer instance found");

			return _signer.GetAddress();
		}

		public ISignerBase GetSigner()
		{
			if (_signer == null) throw new Exception("No signer instance found");

			return _signer;
		}

		public Task<string> GetWalletAddress()
		{
			throw new NotImplementedException();
		}

		public UserInfos GetUserInfos()
		{
			var walletAddress = GetAccount();

			if (string.IsNullOrEmpty(walletAddress)) return null;

			return new UserInfos
			{
				walletAddress = walletAddress
			};
		}
	}
}