﻿using System.Threading.Tasks;
using ComethSDK.Scripts.Tools.Signers.Interfaces;
using ComethSDK.Scripts.Types;
using JetBrains.Annotations;

namespace ComethSDK.Scripts.Adapters.Interfaces
{
	public interface IAuthAdaptor
	{
		public string ChainId { get; }

		public Task Connect([CanBeNull] string walletAddress = "");
		public Task Logout();
		public string GetAccount();
		public ISignerBase GetSigner();
		public Task<string> GetWalletAddress();
		public UserInfos GetUserInfos();
	}
}