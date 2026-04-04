using Finalitika10.Models;

namespace Finalitika10.Services;

public interface ITransactionService
{
    Task<List<TransactionRecord>> GetTransactionsAsync();
    Task SaveTransactionAsync(TransactionRecord transaction);
    Task DeleteTransactionAsync(TransactionRecord transaction);
}

public class TransactionService : ITransactionService
{
    private readonly FinalitikaDatabase _db;

    public TransactionService(FinalitikaDatabase db)
    {
        _db = db;
    }

    public async Task<List<TransactionRecord>> GetTransactionsAsync()
    {
        // База данных мгновенно отдает все транзакции
        return await _db.GetAllAsync<TransactionRecord>();
    }

    public async Task SaveTransactionAsync(TransactionRecord transaction)
    {
        await _db.SaveAsync(transaction);
    }

    public async Task DeleteTransactionAsync(TransactionRecord transaction)
    {
        await _db.DeleteAsync(transaction);
    }
}