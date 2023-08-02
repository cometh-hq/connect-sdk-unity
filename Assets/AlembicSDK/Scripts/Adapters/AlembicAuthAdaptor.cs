using System.Threading.Tasks;
using AlembicSDK.Scripts.Adapters.Interfaces;
using AlembicSDK.Scripts.Core;
using AlembicSDK.Scripts.Interfaces;
using AlembicSDK.Scripts.Tools.Signers;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.Web3.Accounts;
using UnityEngine;

namespace AlembicSDK.Scripts.Adapters
{
	public class AlembicAuthAdaptor : MonoBehaviour, IAuthAdaptor
	{
		[SerializeField] private int chainId;

		private Account _account;
		private EthECKey _ethEcKey;
		private Signer _signer; //TODO: replace with AlembicAuthSigner
		
		private string _jwtToken;

		private void Awake()
		{
			ChainId = chainId.ToString();
		}

		public string ChainId { get; private set; }

		public Task Connect()
		{
			
			return Task.CompletedTask;
		}

		public Task Logout()
		{
			PlayerPrefs.DeleteKey("privateKey");
			_account = null;
			_ethEcKey = null;
			_signer = null;
			return Task.CompletedTask;
		}

		public Account GetAccount()
		{
			return _account;
		}

		public Signer GetSigner()
		{
			return _signer;
		}

		public UserInfo GetUserInfos()
		{
			return null;
		}
	}
}