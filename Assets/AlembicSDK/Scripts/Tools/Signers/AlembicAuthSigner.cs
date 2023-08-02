using System.Collections.Generic;
using System.Threading.Tasks;
using AlembicSDK.Scripts.HTTP;
using AlembicSDK.Scripts.Types.MessageTypes;
using Nethereum.ABI.EIP712;
using Nethereum.Signer.EIP712;

namespace AlembicSDK.Scripts.Core
{
	public class AlembicAuthSigner : Eip712TypedDataSigner
	{
		private string _address;
		private readonly string _jwtToken;
		private readonly API _api;

		public AlembicAuthSigner(string jwtToken, API api)
		{
			_jwtToken = jwtToken;
			_api = api;
		}

		public async Task<string> GetAddress()
		{
			return string.IsNullOrEmpty(_address) ? "" : _address;
		}

		public async Task<string> ConnectSigner()
		{
			_address = await _api.ConnectToAlembicAuth(_jwtToken);
			return _address;
		}

		public async Task<string> SignTypedData(DomainWithChainIdAndVerifyingContract domain,
			Dictionary<string, MemberDescription[]> types, SafeTx value)
		{
			return await _api.SignTypedDataWithAlembicAuth(_jwtToken, domain, types, value);
		}

		public async Task<string> SignTransaction()
		{
			throw new System.NotImplementedException();
		}
		
		public async Task<string> SignMessage(string message)
		{
			throw new System.NotImplementedException();
		}
	}
}