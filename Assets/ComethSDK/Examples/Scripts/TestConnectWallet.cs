using ComethSDK.Scripts.Adapters;
using ComethSDK.Scripts.Core;
using ComethSDK.Scripts.Services;
using ComethSDK.Scripts.Tools;
using Nethereum.Web3;
using TMPro;
using UnityEngine;

namespace ComethSDK.Examples.Scripts
{
	public class TestConnectWallet : TestWallet
	{
		[SerializeField] public ConnectAdaptor authAdaptor;
		[SerializeField] private TMP_Text console;
		[SerializeField] private string walletAddress;
		[SerializeField] private string apiKey;

		private ComethWallet _wallet;

		private void Start()
		{
			if (authAdaptor && !string.IsNullOrEmpty(apiKey))
				_wallet = new ComethWallet(authAdaptor, apiKey);
			else
				Debug.LogError("Please set the apiKey & authAdaptor serialised variables");
		}

		public async void Connect()
		{
			PrintInConsole("Connecting...");
			await _wallet.Connect(walletAddress);
			PrintInConsole("Connected");
		}

		public async void Disconnect()
		{
			PrintInConsole("Disconnecting...");
			await _wallet.Logout();
			PrintInConsole("Disconnected");
		}

		public async void SignMessage()
		{
			PrintInConsole("Signing message...");
			var messageSigned = await _wallet.SignMessage("Hello World!");
			PrintInConsole("Message signed: " + messageSigned);
		}

		public void CancelWait()
		{
			_wallet.CancelWaitingForEvent();
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

		public void GetAddress()
		{
			var address = _wallet.GetAddress();
			PrintInConsole(address);
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

			if (transactionReceipt != null)
			{
				PrintInConsole("Transaction confirmed, see it on the block explorer: " +
				               transactionReceipt.TransactionHash);
				SeeTransactionReceiptOnBlockExplorer(transactionReceipt.TransactionHash, authAdaptor.ChainId);
			}
			else
			{
				PrintInConsole("Issue with event, redirecting to wallet to see the transaction");
				SeeWalletOnBlockExplorer(_wallet.GetAddress(), authAdaptor.ChainId);
			}
		}

		public async void TestCallToCounter()
		{
			const string
				COUNTER_TEST_ADDRESS =
					"0x3633A1bE570fBD902D10aC6ADd65BB11FC914624"; //On polygon : 0x84ADD3fa2c2463C8cF2C95aD70e4b5F602332160";
			var contract = _wallet.GetContract(Constants.COUNTER_ABI, COUNTER_TEST_ADDRESS);
			var counterFunction = contract.GetFunction("counters");
			PrintInConsole("Sending query to get Counter...");
			var counterAmount = await counterFunction.CallAsync<int>(_wallet.GetAddress());
			PrintInConsole("Query successful, Counter = " + counterAmount);
		}

		private async void EstimateGasAndShow(string to, string value, string data)
		{
			var web3 = new Web3(Constants.GetNetworkByChainID(authAdaptor.ChainId).RPCUrl);
			var nonce = await Utils.GetNonce(web3, _wallet.GetAddress());

			var gas = await GasService.CalculateMaxFees(_wallet.GetAddress(), to, value, data, nonce, web3);
			PrintInConsole("Estimated max gas: " + gas);
		}

		private void SeeTransactionReceiptOnBlockExplorer(string txHash, string chainId)
		{
			var url = Constants.GetNetworkByChainID(chainId).BlockExplorerUrl + "/tx/" + txHash;
			Application.OpenURL(url);
		}

		private void SeeWalletOnBlockExplorer(string walletAddressToSee, string chainId)
		{
			var url = Constants.GetNetworkByChainID(chainId).BlockExplorerUrl + "/address/" + walletAddressToSee;
			Application.OpenURL(url);
		}

		private void PrintInConsole(string str)
		{
			console.text += str + "\n";
		}
	}
}