using System;
using ComethSDK.Scripts.Services;
using UnityEngine;

public class TestHash : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		Test();
	}

	private async void Test()
	{
		/*var walletAddress = "0x5B76Bb156C4E9Aa322143d0061AFBd856482648D";
		var privateKey = "0x58476d0865927d3536ee46ad35d36899e5e362cf0825800f453f6ef7c8547dbe";
		var salt = "COMETH-CONNECT";
		
		await EoaFallbackService.SetSignerLocalStorage(walletAddress, privateKey, salt);
		var result = await EoaFallbackService.GetSignerLocalStorage(walletAddress, salt);
		
		Debug.Log("result :"+result);*/
		
		/*
		var result = await EoaFallbackService.EncryptEoaFallback(walletAddress,privateKey,salt);
		Debug.Log("encryptedPrivateKey :"+result.encryptedPrivateKey);
		Debug.Log("iv :"+result.iv);
		
		var result2 = await EoaFallbackService.DecryptEoaFallback(
			walletAddress, 
			Convert.FromBase64String(result.encryptedPrivateKey),
			Convert.FromBase64String(result.iv),
			salt
		);
		
		Debug.Log("result2 :"+result2);*/
	}
}