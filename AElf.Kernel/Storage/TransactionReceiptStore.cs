using AElf.Common;
using AElf.Common.Serializers;
using AElf.Database;

namespace AElf.Kernel.Storage
{
    public class TransactionReceiptStore : KeyValueStoreBase
    {
        public TransactionReceiptStore(IKeyValueDatabase keyValueDatabase, IByteSerializer byteSerializer)
            : base(keyValueDatabase, byteSerializer, GlobalConfig.TransactionReceiptPrefix)
        {
        }
    }
}