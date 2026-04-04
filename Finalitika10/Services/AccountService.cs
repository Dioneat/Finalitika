using Finalitika10.Models;

namespace Finalitika10.Services;

public interface IAccountService
{
    Task<List<BankAccount>> GetAccountsAsync();
    Task SaveAccountAsync(BankAccount account);
    Task DeleteAccountAsync(BankAccount account);
}

public class AccountService : IAccountService
{
    private readonly FinalitikaDatabase _db;

    public AccountService(FinalitikaDatabase db)
    {
        _db = db;
    }

    public async Task<List<BankAccount>> GetAccountsAsync()
    {
        return await _db.GetAllAsync<BankAccount>();
    }

    public async Task SaveAccountAsync(BankAccount account)
    {
        await _db.SaveAsync(account);
    }

    public async Task DeleteAccountAsync(BankAccount account)
    {
        await _db.DeleteAsync(account);
    }
}