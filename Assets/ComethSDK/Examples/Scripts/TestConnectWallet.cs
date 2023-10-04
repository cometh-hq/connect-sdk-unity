using CandyCoded.env;
using ComethSDK.Scripts.Adapters;
using ComethSDK.Scripts.Core;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Types;
using Nethereum.Web3;
using TMPro;
using UnityEngine;

namespace ComethSDK.Examples.Scripts
{
	public class TestConnectWallet : TestWallet
	{
		[SerializeField] public ConnectAdaptor authAdaptor;
		[SerializeField] private TMP_Text console;

		private ComethWallet _wallet;

		private void Start()
		{
			if (env.TryParseEnvironmentVariable("API_KEY", out string apiKey))
				_wallet = new ComethWallet(authAdaptor, apiKey);
			else
				Debug.LogError("API_KEY environment variable not set");
		}

		public async void Connect()
		{
			PrintInConsole("Connecting...");
			await _wallet.Connect();
			PrintInConsole("Connected");
		}

		public async void Disconnect()
		{
			PrintInConsole("Disconnecting...");
			await _wallet.Logout();
			PrintInConsole("Disconnected");
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

		public void ClearBurner()
		{
			PlayerPrefs.DeleteKey("burner-wallet-private-key");
			Debug.Log("Burner wallet cleared");
		}

		public override async void SendTestTransaction(string to)
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

		public void GetUserInfo()
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
			var web3 = new Web3(Constants.GetNetworkByChainID(authAdaptor.ChainId).RPCUrl);
			var nonce = await Utils.GetNonce(web3, _wallet.GetAddress());
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
			var nonce = await Utils.GetNonce(web3, _wallet.GetAddress());

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
			console.text += str + "\n";
		}

		private void PrintUserInfosInConsole(UserInfos userInfos)
		{
			PrintInConsole("walletAddress: " + userInfos.walletAddress);
			PrintInConsole("ownerAddress: " + userInfos.ownerAddress);
		}
	}
}