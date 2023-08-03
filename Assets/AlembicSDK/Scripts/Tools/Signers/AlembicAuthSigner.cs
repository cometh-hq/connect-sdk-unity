using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlembicSDK.Scripts.HTTP;
using AlembicSDK.Scripts.Tools.Signers.Interfaces;
using AlembicSDK.Scripts.Types;
using Nethereum.ABI.EIP712;
using Nethereum.Signer.EIP712;

namespace AlembicSDK.Scripts.Tools.Signers
{
	public class AlembicAuthSigner : Eip712TypedDataSigner, ISignerBase
	{
		private readonly API _api;
		private readonly string _jwtToken;
		private string _address;

		public AlembicAuthSigner(string jwtToken, API api)
		{
			_jwtToken = jwtToken;
			_api = api;
		}

		public string SignTypedData<T, TDomain>(T message, TypedData<TDomain> typedData)
		{
			throw new NotImplementedException();
		}

		public async Task<string> SignTypedData(DomainWithChainIdAndVerifyingContract domain,
			IDictionary<string, MemberDescription[]> types, IDictionary<string, object> value)
		{
			var lowerCaseDomain = new DomainWithChainIdAndVerifyingContractLowerCase
			{
				chainId = domain.ChainId.ToString(),
				verifyingContract = domain.VerifyingContract
			};
			return await _api.SignTypedDataWithAlembicAuth(_jwtToken, lowerCaseDomain, types, value);
		}

		public string GetAddress()
		{
			return string.IsNullOrEmpty(_address) ? "" : _address;
		}

		public async Task<string> ConnectSigner()
		{
			_address = await _api.ConnectToAlembicAuth(_jwtToken);
			return _address;
		}

		public async Task<string> SignTransaction()
		{
			throw new NotImplementedException();
		}

		public async Task<string> SignMessage(string message)
		{
			throw new NotImplementedException();
		}
	}
}