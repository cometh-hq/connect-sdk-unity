using System.Threading.Tasks;
using ComethSDK.Scripts.Adapters;
using ComethSDK.Scripts.Core;
using ComethSDK.Scripts.Interfaces;
using ComethSDK.Scripts.Services;
using ComethSDK.Scripts.Tools;
using ComethSDK.Scripts.Types;
using ComethSDK.Scripts.Types.MessageTypes;
using Nethereum.Web3;
using TMPro;
using UnityEngine;

namespace ComethSDK.Examples.Scripts
{
	public class TestConnectWallet : TestWallet
	{
		[Header("Required")] [SerializeField] private string apiKey;

		[SerializeField] private int chainId;

		[Header("Optional")] [SerializeField] private string walletAddress;

		[SerializeField] private string baseUrl;

		[Header("UI")] [SerializeField] private TMP_Text console;

		private ConnectAdaptor _connectAuthAdaptor;

		private ComethWallet _wallet;

		private void Start()
		{
			if (string.IsNullOrEmpty(apiKey) || chainId == 0)
			{
				Debug.LogError("Please set the apiKey and chainId serialised variables");
				return;
			}

			_connectAuthAdaptor = new ConnectAdaptor(chainId, apiKey, baseUrl);
			_wallet = string.IsNullOrEmpty(baseUrl)
				? new ComethWallet(_connectAuthAdaptor, apiKey)
				: new ComethWallet(_connectAuthAdaptor, apiKey, baseUrl);
		}

		public override async void Connect()
		{
			PrintInConsole("Connecting...");
			if (string.IsNullOrEmpty(walletAddress))
				await _wallet.Connect();
			else
				await _wallet.Connect(walletAddress);
			PrintInConsole("Connected");

			await SafeService.IsDeployed(_wallet.GetAddress(),
				Constants.GetNetworkByChainID(_connectAuthAdaptor.ChainId).RPCUrl);
		}

		public override async void Disconnect()
		{
			PrintInConsole("Disconnecting...");
			await _wallet.Logout();
			PrintInConsole("Disconnected");
		}

		public override async void SignMessage()
		{
			PrintInConsole("Signing message...");
			var messageSigned = await _wallet.SignMessage("Hello World!");
			PrintInConsole("Message signed: " + messageSigned);
		}

		public override void CancelWait()
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
			SeeTransactionReceiptOnBlockExplorer(transactionReceipt.TransactionHash, _connectAuthAdaptor.ChainId);
		}

		public async void SendBatchTestTransaction(string to)
		{
			if (to is "" or Constants.ZERO_ADDRESS)
			{
				Debug.LogError("Please enter a valid address");
				return;
			}

			var dataArr = new[]
			{
				new MetaTransactionData
				{
					to = to,
					value = "0",
					data = "0x00000000"
				},
				new MetaTransactionData
				{
					to = to,
					value = "0",
					data = "0x00000000"
				}
			};

			PrintInConsole("Sending Batch transaction...");
			var safeTxHash = await _wallet.SendBatchTransactions(dataArr);
			PrintInConsole("Safe transaction hash: " + safeTxHash);
			PrintInConsole("Transaction sent, waiting for confirmation...");
			var transactionReceipt = await _wallet.Wait(safeTxHash);
			PrintInConsole("Transaction confirmed, see it on the block explorer: " +
			               transactionReceipt.TransactionHash);
			SeeTransactionReceiptOnBlockExplorer(transactionReceipt.TransactionHash, _connectAuthAdaptor.ChainId);
		}

		public void GetAddress()
		{
			var address = _wallet.GetAddress();
			PrintInConsole(address);
		}

		public override async void TestCallToCount()
		{
			const string
				COUNTER_TEST_ADDRESS =
					"0x3633A1bE570fBD902D10aC6ADd65BB11FC914624"; //On polygon : 0x84ADD3fa2c2463C8cF2C95aD70e4b5F602332160";
			var contract = _wallet.GetContract(Constants.COUNTER_ABI, COUNTER_TEST_ADDRESS);
			var countFunction = contract.GetFunction("count");
			var data = countFunction.GetData();
			var web3 = new Web3(Constants.GetNetworkByChainID(_connectAuthAdaptor.ChainId).RPCUrl);
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
				SeeTransactionReceiptOnBlockExplorer(transactionReceipt.TransactionHash, _connectAuthAdaptor.ChainId);
			}
			else
			{
				PrintInConsole("Issue with event, redirecting to wallet to see the transaction");
				SeeWalletOnBlockExplorer(_wallet.GetAddress(), _connectAuthAdaptor.ChainId);
			}
		}

		public async void TestCallToCountBatch()
		{
			const string
				COUNTER_TEST_ADDRESS =
					"0x3633A1bE570fBD902D10aC6ADd65BB11FC914624"; //On polygon : 0x84ADD3fa2c2463C8cF2C95aD70e4b5F602332160";
			var contract = _wallet.GetContract(Constants.COUNTER_ABI, COUNTER_TEST_ADDRESS);
			var countFunction = contract.GetFunction("count");
			var data = countFunction.GetData();
			EstimateGasAndShow(COUNTER_TEST_ADDRESS, "0", data);

			var dataArr = new[]
			{
				new MetaTransactionData
				{
					to = COUNTER_TEST_ADDRESS,
					value = "0",
					data = data
				},
				new MetaTransactionData
				{
					to = COUNTER_TEST_ADDRESS,
					value = "0",
					data = data
				}
			};

			PrintInConsole("Sending Batch transaction...");
			var safeTxHash = await _wallet.SendBatchTransactions(dataArr);
			PrintInConsole("Safe Transaction hash: " + safeTxHash);
			PrintInConsole("Transaction sent, waiting for confirmation...");
			var transactionReceipt = await _wallet.Wait(safeTxHash);

			if (transactionReceipt != null)
			{
				PrintInConsole("Transaction confirmed, see it on the block explorer: " +
				               transactionReceipt.TransactionHash);
				SeeTransactionReceiptOnBlockExplorer(transactionReceipt.TransactionHash, _connectAuthAdaptor.ChainId);
			}
			else
			{
				PrintInConsole("Issue with event, redirecting to wallet to see the transaction");
				SeeWalletOnBlockExplorer(_wallet.GetAddress(), _connectAuthAdaptor.ChainId);
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

		public async void TestAddRemoveOwners()
		{
			await TestAddOwner();
			await TestRemOwner();
		}

		public async void GetOwners()
		{
			var getOwnersFunction = await _wallet.GetOwners();
			foreach (var owner in getOwnersFunction) Debug.Log(owner);
		}

		public async Task TestAddOwner()
		{
			var newOwner = "0x4C971b2211f6158d474324994C687ba48F040057";
			var getOwnersFunction = await _wallet.GetOwners();
			Debug.Log(getOwnersFunction);

			var addOwnerSafeTxHash = await _wallet.AddOwner(newOwner);
			var transactionReceipt = await _wallet.Wait(addOwnerSafeTxHash);

			if (transactionReceipt != null)
				PrintInConsole("AddOwner Transaction confirmed");
			else
				PrintInConsole("Issue with AddOwner");
		}

		public async Task TestRemOwner()
		{
			var newOwner = "0x4C971b2211f6158d474324994C687ba48F040057";
			var removeOwnerSafeTxHash = await _wallet.RemoveOwner(newOwner);
			var transactionReceipt = await _wallet.Wait(removeOwnerSafeTxHash);

			if (transactionReceipt != null)
				PrintInConsole("RemoveOwner Transaction confirmed");
			else
				PrintInConsole("Issue with RemoveOwner");
		}

		public async void TestEstimateSafeTxGasWithSimulate()
		{
			var value = "0";
			var data = "0x00";
			var to = "0x6e13dA17777a7325DcCF7FAa2358Ef3Db6E452cE";

			EstimateGasAndShowWithSimulate(to, value, data);
			EstimateGasAndShow(to, value, data);
		}

		private async void EstimateGasAndShow(string to, string value, string data)
		{
			var provider = Constants.GetNetworkByChainID(_connectAuthAdaptor.ChainId).RPCUrl;
			var txData = new SafeTx
			{
				to = to,
				value = value,
				data = data
			};

			IMetaTransactionData[] safeTxDataArray = { txData };

			var estimates = await GasService.EstimateTransactionGas(safeTxDataArray, walletAddress, provider);
			PrintInConsole("Estimated safeTxGas Normal: " + estimates);
		}

		private async void EstimateGasAndShowWithSimulate(string to, string value, string data)
		{
			var provider = Constants.GetNetworkByChainID(_connectAuthAdaptor.ChainId).RPCUrl;
			var txData = new SafeTx
			{
				to = to,
				value = value,
				data = data
			};

			var txDataArray = new IMetaTransactionData[] { txData };
			var gas = await GasService.EstimateSafeTxGasWithSimulate(walletAddress, txDataArray, "",
				Constants.GetNetworkByChainID(chainId.ToString()).SafeSingletonAddress,
				Constants.GetNetworkByChainID(chainId.ToString()).SafeTxAccessorAddress,
				provider);

			PrintInConsole("Estimated safeTxGas gas Simulated: " + gas);
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
			Debug.Log(str);
		}

		private async void AddSignerRequestExamples()
		{
			var addSignerRequest = await _connectAuthAdaptor.InitNewSignerRequest(walletAddress);
			var newSignerRequests = await _connectAuthAdaptor.GetNewSignerRequests();

			var newSignerRequest = newSignerRequests[0];
			await _wallet.AddOwner(newSignerRequest.signerAddress);
		}
	}
}