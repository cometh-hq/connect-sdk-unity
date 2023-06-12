# Account Abstraction Unity SDK

Alembic Unity Account Abstraction SDK allows developers to onboard their users with a seedless, gasless experience familiar to Web2.

Account Abstraction (AA) improves transaction user experience by using smart contract wallets as primary accounts.
Our solution is compatible with EIP-4337.

## Instantiate Wallet

To get an API key please [Contact us](https://alembic.tech/)

```C#
[SerializeField] private BurnerWalletAdapter authAdapter; //Set ChainId in inspector
private const string API_KEY = "my_api_key"; 
private AlembicWallet _wallet;

private void Start()
{
    _wallet = new AlembicWallet(authAdapter, API_KEY );
}
```

## Available methods

### Connect

```C#
await _wallet.Connect()
```

This function pops up the social login modal on UI.

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

### Get user infos

```C#
await wallet.GetUserInfos()
```

If the user is logged in with social media accounts, this function can be used to fetch user related data such as email, etc.

### Send transaction

```C#
var safeTxHash = await _wallet.SendTransaction(to, value, data);
```

This function relays the transaction data to the target address. The transaction fees can be sponsored.
Once you have received the SafeTxHash you can wait for the transaction to be mined and receive the transaction receipt like so :

```C#
var transactionReceipt = await _wallet.Wait(safeTxHash);
```

### Get Relay Status

```javascript
const transactionStatus = await wallet.getRelayTxStatus(relayId)
// TransactionStatus:{hash: string,  status: string}
```

Returns the current transaction hash and the status of the relay (sent, mined, confirmed)

### Sign Message

```javascript
var messageSigned = _wallet.SignMessage("Hello World!");
```

Sign the given message using the EOA, owner of the smart wallet.
