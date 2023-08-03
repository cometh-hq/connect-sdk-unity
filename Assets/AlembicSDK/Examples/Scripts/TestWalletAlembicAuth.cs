using AlembicSDK.Scripts.Adapters;
using AlembicSDK.Scripts.Core;
using AlembicSDK.Scripts.Tools;
using AlembicSDK.Scripts.Types;
using CandyCoded.env;
using Nethereum.Web3;
using UnityEngine;

namespace AlembicSDK.Examples.Scripts
{
	public class TestAlembicAuthSigner : MonoBehaviour
	{
		[SerializeField] public AlembicAuthAdaptor authAdaptor;

		private AlembicWallet _wallet;
		private string _apiKey;

		private async void Start()
		{
			if (env.TryParseEnvironmentVariable("API_KEY", out string apiKey))
			{
				_apiKey = apiKey;
			}
			else
			{
				Debug.LogError("API_KEY environment variable not set");
			}

			_wallet = new AlembicWallet(authAdaptor, _apiKey);
			await _wallet.Connect();
		}

		public async void Connect()
		{
			PrintInConsole("Connecting...");
			await _wallet.Connect();
			PrintInConsole("Connected");
		}

		public void SignMessage()
		{
			PrintInConsole("Signing message...");
			var messageSigned = _wallet.SignMessage("Hello World!");
			PrintInConsole("Message signed: " + messageSigned);
		}

		public void CancelWait()
		{
			_wallet.CancelWaitingForEvent();
		}

		public async void SendTestTransaction(string to)
		{
			if (to is "" or Constants.ZERO_ADDRESS)
			{
				Debug.LogError("Please enter a valid address");
				return;
			}

			var value = "0";
			var data = "0x";

			PrintInConsole("Sending transaction...");
			var safeTxHash = await _wallet.SendTransaction(to, value, data);

			if (safeTxHash == Constants.ZERO_ADDRESS)
			{
				PrintInConsole("Transaction failed");
				return;
			}

			PrintInConsole("Safe transaction hash: " + safeTxHash);
			PrintInConsole("Transaction sent, waiting for confirmation...");
			var transactionReceipt = await _wallet.Wait(safeTxHash);
			PrintInConsole("Transaction confirmed, see it on the block explorer: " +
			               transactionReceipt.TransactionHash);
			SeeTransactionReceiptOnBlockExplorer(transactionReceipt.TransactionHash, authAdaptor.ChainId);
		}

		public async void GetUserInfo()
		{
			var userInfos = _wallet.GetUserInfos();
			PrintUserInfosInConsole(userInfos);
		}

		public async void TestCallToCount()
		{
			const string
				COUNTER_TEST_ADDRESS =
					"0x3633A1bE570fBD902D10aC6ADd65BB11FC914624"; //On polygon : 0x84ADD3fa2c2463C8cF2C95aD70e4b5F602332160";
			var contract = _wallet.GetContract(Constants.COUNTER_ABI, COUNTER_TEST_ADDRESS);
			var countFunction = contract.GetFunction("count");
			var data = countFunction.GetData();

			EstimateGasAndShow(COUNTER_TEST_ADDRESS, "0", data);

			PrintInConsole("Sending transaction...");
			var safeTxHash = await _wallet.SendTransaction(COUNTER_TEST_ADDRESS, "0", data);
			PrintInConsole("Safe transaction hash: " + safeTxHash);
			PrintInConsole("Transaction sent, waiting for confirmation...");
			var transactionReceipt = await _wallet.Wait(safeTxHash);
			PrintInConsole("Transaction confirmed, see it on the block explorer: " +
			               transactionReceipt.TransactionHash);
			SeeTransactionReceiptOnBlockExplorer(transactionReceipt.TransactionHash, authAdaptor.ChainId);
		}

		private async void EstimateGasAndShow(string to, string value, string data)
		{
			var web3 = new Web3(Constants.GetNetworkByChainID(authAdaptor.ChainId).RPCUrl);
			var nonce = await AlembicSDK.Scripts.Tools.Utils.GetNonce(web3, _wallet.GetAddress());
			var gas = await _wallet.CalculateMaxFees(to, value, data, nonce);
			PrintInConsole("Estimated max gas: " + gas);
		}

		private void SeeTransactionReceiptOnBlockExplorer(string txHash, string chainId)
		{
			var url = Constants.GetNetworkByChainID(chainId).BlockExplorerUrl + "/tx/" + txHash;
			Application.OpenURL(url);
		}

		private void PrintInConsole(string str)
		{
			Debug.Log(str);
		}

		private void PrintUserInfosInConsole(UserInfos userInfos)
		{
			PrintInConsole("walletAddress: " + userInfos.walletAddress);
			PrintInConsole("ownerAddress: " + userInfos.ownerAddress);
			PrintInConsole("email: " + userInfos.email);
			PrintInConsole("name: " + userInfos.name);
			PrintInConsole("profileImage: " + userInfos.profileImage);
			PrintInConsole("aggregateVerifier: " + userInfos.aggregateVerifier);
			PrintInConsole("verifier: " + userInfos.verifier);
			PrintInConsole("verifierId: " + userInfos.verifierId);
			PrintInConsole("typeOfLogin: " + userInfos.typeOfLogin);
			PrintInConsole("idToken: " + userInfos.idToken);
			PrintInConsole("oAuthIdToken: " + userInfos.oAuthIdToken);
			PrintInConsole("oAuthAccessToken: " + userInfos.oAuthAccessToken);
		}

		/*private async Task SignTypedData(string _walletAddress)
		{

			var safeAddress = await _api.GetPredictedSafeAddress(_walletAddress);
			var domain = new DomainWithChainIdAndVerifyingContract
			{
				ChainId = chainID,
				VerifyingContract = safeAddress
			};

			var types = new Dictionary<string, MemberDescription[]>
			{
				["SafeTx"] = new[]
				{
					new MemberDescription { Name = "to", Type = "address" },
					new MemberDescription { Name = "value", Type = "uint256" },
					new MemberDescription { Name = "data", Type = "bytes" },
					new MemberDescription { Name = "operation", Type = "uint8" },
					new MemberDescription { Name = "safeTxGas", Type = "uint256" },
					new MemberDescription { Name = "baseGas", Type = "uint256" },
					new MemberDescription { Name = "gasPrice", Type = "uint256" },
					new MemberDescription { Name = "gasToken", Type = "address" },
					new MemberDescription { Name = "refundReceiver", Type = "address" },
					new MemberDescription { Name = "nonce", Type = "uint256" }
				}
			};

			var nonce = "0";//await _api.GetNonce(_walletAddress);
			const string to = "0x510c522ebCC6Eb376839E0CFf5D57bb2F422EB8b";
			const string value = "0";
			const string data = "0x";

			var safeTx = AlembicSDK.Scripts.Tools.Utils.CreateSafeTx(to, value, data, int.Parse(nonce));

			var signature = await _api.SignTypedDataWithAlembicAuth(jwtToken, domain, types, safeTx);

			Debug.Log("Signature :" + signature);
		}*/
	}
}