using System.Threading.Tasks;
using AlembicSDK.Scripts.Core;
using AlembicSDK.Scripts.HTTP;
using UnityEngine;

namespace AlembicSDK.Examples.Scripts
{
	public class TestAlembicAuthSigner : MonoBehaviour
	{
		[SerializeField] private string jwtToken;
		[SerializeField] private int chainID;
		[SerializeField] private string apiKey;
		private API _api;

		private AlembicAuthSigner _signer;

		private async void Start()
		{
			_api = new API(apiKey, chainID);
			_signer = new AlembicAuthSigner(jwtToken, _api);

			var address = await ConnectSigner();
			var address2 = await GetAddress();

			if (address != address2) Debug.Log("Wrong address");
		}

		private async Task<string> ConnectSigner()
		{
			var address = await _signer.ConnectSigner();
			Debug.Log("Your address :" + address);

			return address;
		}

		private async Task<string> GetAddress()
		{
			var address = await _signer.GetAddress();
			Debug.Log("Your address :" + address);

			return address;
		}
	}
}