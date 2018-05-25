﻿using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Xunit.Frameworks.Autofac;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Xunit;
using Google.Protobuf;
using AElf.Kernel.Concurrency.Execution;
using AElf.Kernel.Concurrency.Execution.Messages;

namespace AElf.Kernel.Tests.Concurrency.Execution
{

	[UseAutofacTestFramework]
	public class BatchExecutorTest : TestKitBase
	{
		private ActorSystem sys = ActorSystem.Create("test");
		private IChainContext _chainContext;
		private ProtobufSerializer _serializer = new ProtobufSerializer();
		private SmartContractZeroWithTransfer _smartContractZero { get { return (_chainContext.SmartContractZero as SmartContractZeroWithTransfer); } }

		public BatchExecutorTest(ChainContextWithSmartContractZeroWithTransfer chainContext) : base(new XunitAssertions())
		{
			_chainContext = chainContext;
		}

		private Transaction GetTransaction(Hash from, Hash to, ulong qty)
		{
			// TODO: Test with IncrementId
			TransferArgs args = new TransferArgs()
			{
				From = from,
				To = to,
				Quantity = qty
			};

			ByteString argsBS = ByteString.CopyFrom(_serializer.Serialize(args));

			Transaction tx = new Transaction()
			{
				IncrementId = 0,
				From = from,
				To = to,
				MethodName = "Transfer",
				Params = argsBS
			};

			return tx;
		}

		private DateTime GetTransactionStartTime(ITransaction tx)
		{
			TransferArgs args = (TransferArgs)_serializer.Deserialize(tx.Params.ToByteArray(), typeof(TransferArgs));
			return _smartContractZero.TransactionStartTimes[args];
		}

		private DateTime GetTransactionEndTime(ITransaction tx)
		{
			TransferArgs args = (TransferArgs)_serializer.Deserialize(tx.Params.ToByteArray(), typeof(TransferArgs));
			return _smartContractZero.TransactionEndTimes[args];
		}

		[Fact]
		public void TwoJobBatchExecutionTest()
		{
			TwoJobBatchExecutionTestWithChildType(BatchExecutor.ChildType.Group);
			TwoJobBatchExecutionTestWithChildType(BatchExecutor.ChildType.Job);
		}

		public void TwoJobBatchExecutionTestWithChildType(BatchExecutor.ChildType childType)
		{
			/*
			 *  Job 1: (0-1, 10), (1-2, 9)
			 *  Job 2: (3-4, 8)
			 */

			var balances = new List<int>()
			{
				100, 0, 0, 200, 0
			};
			var addresses = Enumerable.Range(0, balances.Count).Select(x => Hash.Generate()).ToList();

			foreach (var addbal in addresses.Zip(balances, Tuple.Create))
			{
				_smartContractZero.SetBalance(addbal.Item1, (ulong)addbal.Item2);
			}

			var txs = new List<ITransaction>(){
				GetTransaction(addresses[0], addresses[1], 10),
				GetTransaction(addresses[1], addresses[2], 9),
				GetTransaction(addresses[3], addresses[4], 8)
			};
			var txsHashes = txs.Select(y => y.GetHash()).ToList();

			var finalBalances = new List<int>
			{
				90, 1, 9, 192, 8
			};

			var executor1 = sys.ActorOf(BatchExecutor.Props(_chainContext, txs, TestActor, childType));
			Watch(executor1);
			executor1.Tell(StartExecutionMessage.Instance);
			var results = new List<TransactionResult>()
			{
				ExpectMsg<TransactionResultMessage>().TransactionResult,
				ExpectMsg<TransactionResultMessage>().TransactionResult,
				ExpectMsg<TransactionResultMessage>().TransactionResult,
			}.OrderBy(y => txsHashes.IndexOf(y.TransactionId)).ToList();
			ExpectTerminated(executor1);
			// Job 1: Tx0 -> Tx1 (Tx1 starts after Tx0 finishes)
			// Job 2: Tx2 (Tx2 starts before Tx1 finishes, not strict, but should be)
			Assert.True(GetTransactionStartTime(txs[1]) > GetTransactionEndTime(txs[0]));
			Assert.True(GetTransactionStartTime(txs[2]) < GetTransactionEndTime(txs[1]));
			Assert.Equal(Status.Mined, results[0].Status);
			Assert.Equal(Status.Mined, results[1].Status);
			Assert.Equal(Status.Mined, results[2].Status);
			foreach (var addFinbal in addresses.Zip(finalBalances, Tuple.Create))
			{
				Assert.Equal((ulong)addFinbal.Item2, _smartContractZero.GetBalance(addFinbal.Item1));
			}
		}
	}
}
