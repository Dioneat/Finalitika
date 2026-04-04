using Finalitika10.Models;

namespace Finalitika10.Services;

public interface ICategoryService
{
    Task<List<TransactionCategory>> GetCategoriesAsync();
    Task SaveCategoryAsync(TransactionCategory category);
    Task DeleteCategoryAsync(TransactionCategory category);
}

public class CategoryService : ICategoryService
{
    private readonly FinalitikaDatabase _db;

    public CategoryService(FinalitikaDatabase db)
    {
        _db = db;
    }

    public async Task<List<TransactionCategory>> GetCategoriesAsync()
    {
        var categories = await _db.GetAllAsync<TransactionCategory>();

        if (!categories.Any())
        {
            await InitializeDefaultCategoriesAsync();
            categories = await _db.GetAllAsync<TransactionCategory>();
        }

        return categories;
    }

    public async Task SaveCategoryAsync(TransactionCategory category)
    {
        await _db.SaveAsync(category);
    }

    public async Task DeleteCategoryAsync(TransactionCategory category)
    {
        await _db.DeleteAsync(category);
    }

    private async Task InitializeDefaultCategoriesAsync()
    {
        var defaults = new List<TransactionCategory>
        {
            new TransactionCategory { Name = "Продукты", Icon = "🛒", ColorHex = "#27AE60", Type = "Расход", IsDefault = true },
            new TransactionCategory { Name = "Транспорт", Icon = "🚕", ColorHex = "#F39C12", Type = "Расход", IsDefault = true },
            new TransactionCategory { Name = "Жилье", Icon = "🏠", ColorHex = "#34495E", Type = "Расход", IsDefault = true },
            new TransactionCategory { Name = "Здоровье", Icon = "💊", ColorHex = "#E74C3C", Type = "Расход", IsDefault = true },
            new TransactionCategory { Name = "Зарплата", Icon = "💰", ColorHex = "#27AE60", Type = "Доход", IsDefault = true },
            new TransactionCategory { Name = "Прочее", Icon = "📦", ColorHex = "#95A5A6", Type = "Расход", IsDefault = true },
            new TransactionCategory { Name = "Прочее", Icon = "📦", ColorHex = "#95A5A6", Type = "Доход", IsDefault = true }
        };

        foreach (var cat in defaults)
        {
            await _db.SaveAsync(cat);
        }
    }
}