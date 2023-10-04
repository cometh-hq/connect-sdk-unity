using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComethSDK.Scripts.HTTP;
using ComethSDK.Scripts.Tools.Signers.Interfaces;
using ComethSDK.Scripts.Types;
using Nethereum.ABI.EIP712;
using Nethereum.Signer.EIP712;

namespace ComethSDK.Scripts.Tools.Signers
{
	public class ComethAuthSigner : Eip712TypedDataSigner, ISignerBase
	{
		private readonly API _api;
		private readonly string _jwtToken;
		private string _address;

		public ComethAuthSigner(string jwtToken, API api)
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
				chainId = domain.ChainId,
				verifyingContract = domain.VerifyingContract
			};
			if (types.ContainsKey("EIP712Domain")) types.Remove("EIP712Domain");
			return await _api.SignTypedDataWithComethAuth(_jwtToken, lowerCaseDomain, types, value);
		}

		public string GetAddress()
		{
			return string.IsNullOrEmpty(_address) ? "" : _address;
		}

		public async Task ConnectSigner()
		{
			_address = await _api.ConnectToComethAuth(_jwtToken);
		}
	}
}