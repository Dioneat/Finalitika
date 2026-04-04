using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Globalization;

namespace Finalitika10.Services.Import
{
    public class TinkoffCsvParser : IStatementParser
    {
        private class TinkoffRecord
        {
            [Index(0)] public string Date { get; set; }
            [Index(2)] public string CardNumber { get; set; }
            [Index(3)] public string Status { get; set; }
            [Index(4)] public string Amount { get; set; }
            [Index(5)] public string Currency { get; set; }
            [Index(9)] public string Category { get; set; }
            [Index(10)] public string Mcc { get; set; }
            [Index(11)] public string Description { get; set; }
            [Index(13)] public string InvestRounding { get; set; }
        }

        public async Task<List<ImportedTransaction>> ParseAsync(Stream fileStream)
        {
            var result = new List<ImportedTransaction>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecordsAsync<TinkoffRecord>();

            await foreach (var record in records)
            {
                if (string.IsNullOrWhiteSpace(record.Date) || string.IsNullOrWhiteSpace(record.Amount))
                    continue;

                if (DateTime.TryParseExact(record.Date, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date) &&
                    double.TryParse(record.Amount, NumberStyles.Any, CultureInfo.GetCultureInfo("ru-RU"), out double amount))
                {
                    var transaction = new ImportedTransaction
                    {
                        Date = date,
                        Amount = amount,
                        Currency = record.Currency ?? "RUB",
                        Description = record.Description ?? "",
                        Category = record.Category ?? ""
                    };

                    if (!string.IsNullOrWhiteSpace(record.CardNumber)) transaction.AdditionalInfo.Add("CardNumber", record.CardNumber);
                    if (!string.IsNullOrWhiteSpace(record.Status)) transaction.AdditionalInfo.Add("Status", record.Status);
                    if (!string.IsNullOrWhiteSpace(record.Mcc)) transaction.AdditionalInfo.Add("MCC", record.Mcc);
                    if (!string.IsNullOrWhiteSpace(record.InvestRounding)) transaction.AdditionalInfo.Add("InvestRounding", record.InvestRounding);

                    result.Add(transaction);
                }
            }

            return result;
        }
    }
}