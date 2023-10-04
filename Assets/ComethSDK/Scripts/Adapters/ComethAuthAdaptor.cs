using System;
using System.Threading.Tasks;
using ComethSDK.Scripts.Adapters.Interfaces;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Tools.Signers;
using ComethSDK.Scripts.Tools.Signers.Interfaces;
using ComethSDK.Scripts.Types;
using UnityEngine;

namespace ComethSDK.Scripts.Adapters
{
	public class ComethAuthAdaptor : MonoBehaviour, IAuthAdaptor
	{
		[SerializeField] private int chainId;
		[SerializeField] private string jwtToken;
		[SerializeField] private string apiKey;

		private string _account;
		private API _api;
		private ComethAuthSigner _signer;

		private void Awake()
		{
			if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(apiKey) || chainId == 0)
				Debug.LogError("Serialized fields empty");

			ChainId = chainId.ToString();
			_api = new API(apiKey, chainId);
		}

		public string ChainId { get; private set; }

		public async Task Connect()
		{
			_signer = new ComethAuthSigner(jwtToken, _api);
			await _signer.ConnectSigner();
			Debug.Log("Connected to signer");
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