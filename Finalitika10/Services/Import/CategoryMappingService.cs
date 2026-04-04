using Finalitika10.Models;

namespace Finalitika10.Services.Import
{
    public class CategoryMappingService
    {
        private readonly Dictionary<string, string> _mccMap = new()
        {
            { "5411", "Продукты" }, { "5499", "Продукты" },
            { "5812", "Кафе и фастфуд" }, { "5814", "Кафе и фастфуд" },
            { "4111", "Транспорт" }, { "4121", "Транспорт" }, { "4131", "Транспорт" },
            { "5541", "Автомобиль" }, { "5533", "Автомобиль" },
            { "5912", "Здоровье" }, { "8011", "Здоровье" }, { "8021", "Здоровье" },
            { "5651", "Одежда" }, { "5691", "Одежда" }, { "5621", "Одежда" },
            { "5816", "Развлечения" }, { "7832", "Развлечения" },
            { "4814", "Коммуналка и связь" }, { "4900", "Коммуналка и связь" }
        };

        public TransactionCategory MapTransaction(Services.Import.ImportedTransaction importedTx, List<TransactionCategory> userCategories)
        {
            string targetCategoryName = null;

            if (importedTx.AdditionalInfo.TryGetValue("MCC", out string mcc) && _mccMap.ContainsKey(mcc))
            {
                targetCategoryName = _mccMap[mcc];
            }

            if (string.IsNullOrEmpty(targetCategoryName))
            {
                var desc = importedTx.Description.ToLower();
                if (desc.Contains("такси") || desc.Contains("yandex go") || desc.Contains("uber")) targetCategoryName = "Транспорт";
                else if (desc.Contains("аптека") || desc.Contains("zdravcity") || desc.Contains("eapteka")) targetCategoryName = "Здоровье";
                else if (desc.Contains("пятерочка") || desc.Contains("магнит") || desc.Contains("вкусвилл")) targetCategoryName = "Продукты";
                else if (desc.Contains("steam") || desc.Contains("psn") || desc.Contains("kino")) targetCategoryName = "Развлечения";
            }

            string expectedType = importedTx.Amount < 0 ? "Расход" : "Доход";

            if (!string.IsNullOrEmpty(targetCategoryName))
            {
                var matched = userCategories.FirstOrDefault(c => c.Name == targetCategoryName && c.Type == expectedType);
                if (matched != null) return matched;
            }

            return userCategories.FirstOrDefault(c => c.Name == "Прочее" && c.Type == expectedType);
        }
    }
}