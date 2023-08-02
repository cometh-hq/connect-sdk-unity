using System;
using System.Threading.Tasks;
using AlembicSDK.Scripts.Adapters.Interfaces;
using AlembicSDK.Scripts.Core;
using AlembicSDK.Scripts.HTTP;
using AlembicSDK.Scripts.Tools.Signers;
using AlembicSDK.Scripts.Types;
using UnityEngine;

namespace AlembicSDK.Scripts.Adapters
{
	public class AlembicAuthAdaptor : MonoBehaviour, IAuthAdaptor
	{
		[SerializeField] private int chainId;
		[SerializeField] private string jwtToken;
		[SerializeField] private string apiKey;

		private string _account;
		private API _api;
		private AlembicAuthSigner _signer;

		private void Awake()
		{
			if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(apiKey) || chainId == 0)
				Debug.LogError("Serialized fields empty");

			ChainId = chainId.ToString();
			_api = new API(apiKey, chainId);
		}

		public string ChainId { get; private set; }

		public Task Connect()
		{
			_signer = new AlembicAuthSigner(jwtToken, _api);
			return Task.CompletedTask;
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

			return _account;
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
				walletAddress = GetAccount()
			};
		}
	}
}