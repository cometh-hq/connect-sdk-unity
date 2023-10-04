using System;
using System.Threading.Tasks;
using ComethSDK.Scripts.Adapters.Interfaces;
using ComethSDK.Scripts.Tools.Signers;
using ComethSDK.Scripts.Tools.Signers.Interfaces;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using UnityEngine;

namespace ComethSDK.Scripts.Adapters
{
	[RequireComponent(typeof(Web3Auth))]
	public class Web3AuthAdaptor : MonoBehaviour, IAuthAdaptor
	{
		private const Provider SelectedProvider = Provider.GOOGLE;
		[SerializeField] private string clientId;
		[SerializeField] private string redirectUrl;
		[SerializeField] private Web3Auth.Network web3AuthNetwork;
		[SerializeField] private int chainId;

		private string _account;
		private EthECKey _ethEcKey;

		private Signer _signer;
		private UserInfo _userInfo;
		private TaskCompletionSource<bool> _waitingTaskCompletionSource = new();
		private Web3Auth _web3Auth;

		private void Awake()
		{
			_web3Auth = GetComponent<Web3Auth>();
			_web3Auth.Init(clientId, redirectUrl, web3AuthNetwork);
			_web3Auth.setOptions(new Web3AuthOptions
			{
				redirectUrl = new Uri(redirectUrl),
				clientId = clientId,
				network = web3AuthNetwork
			});

			_web3Auth.onLogin += onLogin;
			_web3Auth.onLogout += onLogout;

			ChainId = chainId.ToString();
		}

		public string ChainId { get; private set; }

		public async Task Connect()
		{
			_waitingTaskCompletionSource = new TaskCompletionSource<bool>();
			var options = new LoginParams
			{
				loginProvider = SelectedProvider
			};

			Debug.Log("Logging in with provider: " + SelectedProvider);
			_web3Auth.login(options);

			Debug.Log("Waiting for login...");
			await _waitingTaskCompletionSource.Task;
			Debug.Log("Login completed!");
		}

		public async Task Logout()
		{
			_waitingTaskCompletionSource = new TaskCompletionSource<bool>();

			Debug.Log("Logging out...");
			_web3Auth.logout();

			Debug.Log("Waiting for logout...");
			await _waitingTaskCompletionSource.Task;
			Debug.Log("Logout completed!");
		}

		public string GetAccount()
		{
			return _account;
		}

		public ISignerBase GetSigner()
		{
			return _signer;
		}

		public UserInfo GetUserInfos()
		{
			return _userInfo;
		}

		private void onLogin(Web3AuthResponse response)
		{
			_userInfo = response.userInfo;
			_ethEcKey = new EthECKey(response.privKey);
			var eoa = new Account(response.privKey);
			_account = eoa.Address;
			_signer = new Signer(_ethEcKey);

			Debug.Log("Logged in with account :" + _account);
			_waitingTaskCompletionSource.SetResult(true);
		}

		private void onLogout()
		{
			_userInfo = null;
			_account = null;
			_ethEcKey = null;

			Debug.Log("Logged out!");
		}
	}
}