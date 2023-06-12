using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AlembicSDK.Examples.Scripts
{
	public class SendTransactionButton : MonoBehaviour
	{
		[SerializeField] private TMP_InputField to;
		[SerializeField] private TestWallet testWallet;

		private Button _button;

		private void Start()
		{
			_button = GetComponent<Button>();
			_button.onClick.AddListener(OnClick);
		}

		private void OnClick()
		{
			testWallet.SendTestTransaction(to.text);
		}
	}
}