# Cometh Connect Unity SDK

Cometh Connect SDK allows developers to onboard their users with a seedless, gasless experience familiar to Web2.
Account Abstraction (AA) improves transaction user experience by using smart contract wallets as primary accounts.

To get an API key please [Contact us](https://cometh.io/)

## Test Cometh Connect SDK

Open the scene: **Examples/Scenes/Connect-Example.unity**

Click on the TestWallet GameObject:

  * Fill your API-KEY
  * To use a specific wallet fill its address. If you don't specify an address a new wallet will be created. The address wille be printed in the console log.

Launch the Scene.

  * First click on "Connect" button. If you did not specified a wallet address, you will get a new one. Before you made a first transaction your wallet is not deployed.
  * Click on "Send Sponsored Tx: Count" to call a sponsored transaction on a "Count" contract.
  * To see the number of time you clicked on "Count" , use "Display Count"

To see usage of the SDK open the script: **Examples/Scripts/TestConnectWallet.cs**

## Instantiate Wallet

```C#
[SerializeField] private ConnectAdaptor authAdapter; //Set ChainId in inspector
[SerializeField] private const string API_KEY = "my_api_key"; //Set API_KEY in inspector
private ComethWallet _wallet;

private void Start()
{
    _wallet = new ComethWallet(authAdapter, API_KEY );
}
```

## Available methods

### Connect

When your user doesn't already have a wallet, create a new one by calling the connect method without parameter.

```C#
await _wallet.Connect()
```

When you already have created your user's wallet, just pass the wallet address to the connect method in order to instanciate it.

```C#
await _wallet.Connect(WALLET_ADDRESS)
```

### Logout

```C#
await _wallet.Logout()
```

This function logs the user out and clears the cache.

### Get Address

```C#
await _wallet.GetAddress()
```

This function returns the address of the wallet.

### Send transaction

```C#
var safeTxHash = await _wallet.SendTransaction(to, value, data);
```

This function relays the transaction data to the target address. The transaction fees can be sponsored.
Once you have received the SafeTxHash you can wait for the transaction to be mined and receive the transaction receipt like so :

```C#
var transactionReceipt = await _wallet.Wait(safeTxHash);
```

### Sign Message

```C#
var messageSigned = await _wallet.SignMessage("Hello World!");
```

Sign the given message using the EOA, owner of the smart wallet.
