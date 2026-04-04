using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Globalization;

namespace Finalitika10.Services.Import
{
    public class AlfaBankCsvParser : IStatementParser
    {
        private class AlfaRecord
        {
            [Index(0)] public string OperationDate { get; set; }
            [Index(5)] public string CardNumber { get; set; }
            [Index(6)] public string Merchant { get; set; }
            [Index(7)] public string Amount { get; set; }
            [Index(8)] public string Currency { get; set; }
            [Index(9)] public string Status { get; set; }
            [Index(10)] public string Category { get; set; }
            [Index(11)] public string Mcc { get; set; }
            [Index(12)] public string Type { get; set; } 
        }

        public async Task<List<ImportedTransaction>> ParseAsync(Stream fileStream)
        {
            var result = new List<ImportedTransaction>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                MissingFieldFound = null, 
                BadDataFound = null      
            };

            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecordsAsync<AlfaRecord>();

            await foreach (var record in records)
            {
                if (string.IsNullOrWhiteSpace(record.OperationDate) || string.IsNullOrWhiteSpace(record.Amount))
                    continue;

                if (DateTime.TryParseExact(record.OperationDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date) &&
                    double.TryParse(record.Amount.Replace(".", ","), out double amount))
                {
                    if (record.Type?.Equals("Списание", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        amount = -amount;
                    }

                    var transaction = new ImportedTransaction
                    {
                        Date = date,
                        Amount = amount,
                        Currency = record.Currency ?? "RUB",
                        Description = record.Merchant ?? "",
                        Category = record.Category ?? ""
                    };

                    if (!string.IsNullOrWhiteSpace(record.CardNumber)) transaction.AdditionalInfo.Add("CardNumber", record.CardNumber);
                    if (!string.IsNullOrWhiteSpace(record.Status)) transaction.AdditionalInfo.Add("Status", record.Status);
                    if (!string.IsNullOrWhiteSpace(record.Mcc)) transaction.AdditionalInfo.Add("MCC", record.Mcc);

                    result.Add(transaction);
                }
            }

            return result;
        }
    }
}
