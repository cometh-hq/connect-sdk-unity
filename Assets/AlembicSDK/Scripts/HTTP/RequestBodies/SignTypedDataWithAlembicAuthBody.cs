﻿using System.Collections.Generic;
using AlembicSDK.Scripts.Types;
using AlembicSDK.Scripts.Types.MessageTypes;
using Nethereum.ABI.EIP712;

namespace AlembicSDK.Scripts.HTTP.RequestBodies
{
	public class SignTypedDataWithAlembicAuthBody
	{
		public DomainWithChainIdAndVerifyingContractLowerCase domain { get; set; }
		public IDictionary<string, MemberDescription[]> types { get; set; }
		public IDictionary<string, object> value { get; set; }
	}
}