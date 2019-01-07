using System;
using System.Collections.Generic;
using AElf.Common;
using AElf.Kernel;
using AElf.Kernel.Managers;
using AElf.Sdk.CSharp.State;
using AElf.SmartContract;
using Google.Protobuf;

namespace AElf.Sdk.CSharp
{
    public partial class CSharpSmartContract<TContractState> : CSharpSmartContractAbstract
        where TContractState : ContractState
    {
        internal override void SetSmartContractContext(ISmartContractContext smartContractContext)
        {
            _context.SmartContractContext = smartContractContext;
        }

        internal override void SetTransactionContext(ITransactionContext transactionContext)
        {
            _context.TransactionContext = transactionContext;
            SetContractAddress(transactionContext.Transaction.To);
        }

        internal override void SetStateManager(IStateManager stateManager)
        {
            State.Manager = stateManager;
        }

        internal override void SetContractAddress(Address address)
        {
            var path = new StatePath();
            path.Path.Add(ByteString.CopyFromUtf8(address.GetFormatted()));
            State.Path = path;
        }

        internal override Dictionary<StatePath, StateValue> GetChanges()
        {
            return State.GetChanges();
        }
        internal override void Cleanup()
        {
            State.Clear();
        }
    }
}