using SQLite;
using Finalitika10.Models;

namespace Finalitika10.Services;

public class FinalitikaDatabase
{
    private SQLiteAsyncConnection _database;
    private readonly string _dbPath;

    public FinalitikaDatabase(string dbPath)
    {
        _dbPath = dbPath;
    }

    private async Task InitAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(_dbPath);

        await _database.CreateTableAsync<BankAccount>();
        await _database.CreateTableAsync<TransactionRecord>();
        await _database.CreateTableAsync<TransactionCategory>();
        await _database.CreateTableAsync<PaymentEvent>(); 
    }


    public async Task<List<T>> GetAllAsync<T>() where T : new()
    {
        await InitAsync();
        return await _database.Table<T>().ToListAsync();
    }

    public async Task<int> SaveAsync<T>(T item) where T : new()
    {
        await InitAsync();
        return await _database.InsertOrReplaceAsync(item);
    }

    public async Task<int> DeleteAsync<T>(T item) where T : new()
    {
        await InitAsync();
        return await _database.DeleteAsync(item);
    }

    public async Task<List<TransactionRecord>> GetTransactionsByAccountAsync(string accountId)
    {
        await InitAsync();
        return await _database.Table<TransactionRecord>().Where(t => t.AccountId == accountId).ToListAsync();
    }
}