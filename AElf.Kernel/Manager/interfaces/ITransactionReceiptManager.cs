using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Common;

namespace AElf.Kernel.Manager.Interfaces
{
    public interface ITransactionReceiptManager
    {
        Task AddOrUpdateReceiptAsync(TransactionReceipt receipt);
        Task AddOrUpdateReceiptsAsync(IEnumerable<TransactionReceipt> receipts);
        Task<TransactionReceipt> GetReceiptAsync(Hash txId);
    }
}