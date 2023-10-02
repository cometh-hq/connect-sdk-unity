using System;
using System.Threading.Tasks;
using AlembicSDK.Scripts.Adapters.Interfaces;
using AlembicSDK.Scripts.HTTP;
using AlembicSDK.Scripts.Services;
using AlembicSDK.Scripts.Tools.Signers;
using AlembicSDK.Scripts.Tools.Signers.Interfaces;
using AlembicSDK.Scripts.Types;
using UnityEngine;

namespace AlembicSDK.Scripts.Adapters
{
	public class ConnectAdaptor : MonoBehaviour, IAuthAdaptor
	{
		[SerializeField] private int chainId;
		[SerializeField] private string jwtToken;
		[SerializeField] private string apiKey;

		[SerializeField] private string userName;
		[SerializeField] private string rpcUrl;
		[SerializeField] private string baseUrl;

		private string _account;
		private API _api;
		private AlembicAuthSigner _signer;

		private void Awake()
		{
			if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(apiKey) || chainId == 0 ||
			    string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(rpcUrl) || string.IsNullOrEmpty(baseUrl))
				Debug.LogError("Serialized fields empty");

			ChainId = chainId.ToString();
			_api = new API(apiKey, chainId);
		}

		public string ChainId { get; private set; }

		public async Task Connect()
		{
			var walletAddress = await _api.GetWalletAddressFromUserID(jwtToken);
			var decodedToken = tokenService.decodeToken(jwtToken);
			var userId = decodedToken?.payload.sub;
			_signer = BurnerWalletService.CreateOrGetSigner(jwtToken, userId, walletAddress, _api);
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

		public UserInfo GetUserInfos()
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