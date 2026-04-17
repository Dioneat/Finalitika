using SQLite;
using Finalitika10.Models;

namespace Finalitika10.Services;

public class FinalitikaDatabase
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public FinalitikaDatabase(string dbPath)
    {
        _dbPath = dbPath;
    }

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(_dbPath);

        await _database.CreateTableAsync<AIChatHistoryEntity>();
        await _database.CreateTableAsync<BankAccount>();
        await _database.CreateTableAsync<TransactionRecord>();
        await _database.CreateTableAsync<TransactionCategory>();
        await _database.CreateTableAsync<PaymentEvent>();

        await TryAddColumnAsync("AiChatHistory", "EncryptedTextBase64", "TEXT");
        await TryAddColumnAsync("AiChatHistory", "NonceBase64", "TEXT");
        await TryAddColumnAsync("AiChatHistory", "TagBase64", "TEXT");
        await TryAddColumnAsync("AiChatHistory", "EncryptionVersion", "INTEGER NOT NULL DEFAULT 0");
    }

    private async Task TryAddColumnAsync(string tableName, string columnName, string columnTypeSql)
    {
        try
        {
            await _database!.ExecuteAsync(
                $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnTypeSql}");
        }
        catch
        {
        }
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        await InitAsync();
        return _database!;
    }

    public async Task<List<T>> GetAllAsync<T>() where T : new()
    {
        await InitAsync();
        return await _database!.Table<T>().ToListAsync();
    }

    public async Task<int> SaveAsync<T>(T item) where T : new()
    {
        await InitAsync();
        return await _database!.InsertOrReplaceAsync(item);
    }

    public async Task<int> DeleteAsync<T>(T item) where T : new()
    {
        await InitAsync();
        return await _database!.DeleteAsync(item);
    }

    public async Task<List<TransactionRecord>> GetTransactionsByAccountAsync(string accountId)
    {
        await InitAsync();
        return await _database!
            .Table<TransactionRecord>()
            .Where(t => t.AccountId == accountId)
            .ToListAsync();
    }
}