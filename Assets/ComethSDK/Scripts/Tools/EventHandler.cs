﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.Contracts;
using Nethereum.GnosisSafe.ContractDefinition;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using UnityEngine;

namespace ComethSDK.Scripts.Tools
{
	public class EventHandler
	{
		private readonly Event<ExecutionFailureEventDTO> _execFailureEventHandler;
		private readonly Event<ExecutionSuccessEventDTO> _execSuccessEventHandler;
		private readonly Web3 _web3;

		private bool _cancelled;
		private float _timerCurrent;
		private float _timerMax;


		public EventHandler(Web3 web3, string safeAddress, float timerMax)
		{
			_web3 = web3;
			_timerMax = timerMax;
			_execSuccessEventHandler = _web3.Eth.GetEvent<ExecutionSuccessEventDTO>(safeAddress);
			_execFailureEventHandler = _web3.Eth.GetEvent<ExecutionFailureEventDTO>(safeAddress);

			var filterExecSuccess = _execSuccessEventHandler.CreateFilterInput();
			_execSuccessEventHandler.GetAllChangesAsync(filterExecSuccess);

			var filterExecFailure = _execFailureEventHandler.CreateFilterInput();
			_execFailureEventHandler.GetAllChangesAsync(filterExecFailure);

			Debug.Log("Event Handler Created");
		}

		//this function can be optimized / can remove boilerplate
		public async Task<TransactionReceipt> Wait(string safeTxHash)
		{
			//set it to false in case it was cancelled before
			_cancelled = false;
			_timerCurrent = 0;

			var safeTxHashBytes = safeTxHash.RemoveHexPrefix().HexToByteArray();

			var txSuccessEventFound = false;
			var txFailureEventFound = false;

			EventLog<ExecutionSuccessEventDTO> txSuccessEvent = null;
			EventLog<ExecutionFailureEventDTO> txFailureEvent = null;

			Debug.Log("Waiting for event");
			try
			{
				while (!txFailureEventFound && !txSuccessEventFound && !_cancelled)
				{
					var currentBlockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
					var oldBlockNumber = (currentBlockNumber.Value + Constants.BLOCK_EVENT_GAP).ToHexBigInteger();

					var filterExecSuccess = _execSuccessEventHandler.CreateFilterInput(
						new BlockParameter(oldBlockNumber), null);
					var allSuccessfulEventsFound = await _execSuccessEventHandler.GetAllChangesAsync(filterExecSuccess);

					foreach (var events in allSuccessfulEventsFound)
					{
						if (!safeTxHashBytes.SequenceEqual(events.Event.TxHash)) continue;
						
						Debug.Log("Success");
						txSuccessEvent = events;
						txSuccessEventFound = true;
						break;
					}
						
					var filterExecFailure = _execFailureEventHandler.CreateFilterInput(
						new BlockParameter(oldBlockNumber), null);
					var allFailureEventsFound =
						await _execFailureEventHandler.GetAllChangesAsync(filterExecFailure);
					foreach (var events in allFailureEventsFound)
					{
						if (!safeTxHashBytes.SequenceEqual(events.Event.TxHash)) continue;
						
						Debug.Log("Failure");
						txFailureEvent = events;
						txFailureEventFound = true;
						break;
					}
					
					_timerCurrent += Time.deltaTime;
					if (_timerCurrent >= _timerMax)
					{
						Debug.Log("Timeout");
						return null;
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning(e);
				return null;
			}

			if (_cancelled)
			{
				_cancelled = false;
				Debug.Log("Cancelled");
				return null;
			}

			if (txSuccessEventFound)
			{
				TransactionReceipt txResponse = null;
				while (txResponse == null)
					txResponse =
						await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txSuccessEvent.Log
							.TransactionHash);

				return txResponse;
			}
			else //txFailureEvent here is guaranteed to be found
			{
				TransactionReceipt txResponse = null;
				while (txResponse == null)
					txResponse =
						await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txFailureEvent.Log
							.TransactionHash);

				return txResponse;
			}
		}

		public void CancelWait()
		{
			_cancelled = true;
		}
	}
}