using System.Threading.Tasks;
using Nethereum.Contracts;
using Nethereum.Web3;

namespace ComethSDK.Scripts.Services
{
	public class SimulateTxAcessorService
	{
		private const string _abi = "[{inputs:[],stateMutability:\"nonpayable\",type:\"constructor\",},{inputs:[{internalType:\"address\",name:\"to\",type:\"address\",},{internalType:\"uint256\",name:\"value\",type:\"uint256\",},{internalType:\"bytes\",name:\"data\",type:\"bytes\",},{internalType:\"enumEnum.Operation\",name:\"operation\",type:\"uint8\",},],name:\"simulate\",outputs:[{internalType:\"uint256\",name:\"estimate\",type:\"uint256\",},{internalType:\"bool\",name:\"success\",type:\"bool\",},{internalType:\"bytes\",name:\"returnData\",type:\"bytes\",},],stateMutability:\"nonpayable\",type:\"function\",},]";
		
		public static Contract GetContract(string address, string provider)
		{
			var web3 = new Web3(provider);
			return web3.Eth.GetContract(_abi, address);
		}
	}
}