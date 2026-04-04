using ExcelDataReader;
using System.Globalization;

namespace Finalitika10.Services.Import
{
    public class AlfaBankXlsxParser : IStatementParser
    {
        public async Task<List<ImportedTransaction>> ParseAsync(Stream fileStream)
        {
            var result = new List<ImportedTransaction>();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            await Task.Run(() =>
            {
                using var reader = ExcelReaderFactory.CreateReader(fileStream);

                reader.Read();

                while (reader.Read())
                {
                    if (reader.GetValue(0) == null || reader.GetValue(7) == null) continue;

                    DateTime date = GetDateFromCell(reader.GetValue(0));
                    if (date == DateTime.MinValue) continue;

                    double amount = GetDoubleFromCell(reader.GetValue(7));

                    string type = reader.GetValue(12)?.ToString() ?? "";
                    if (type.Equals("Списание", StringComparison.OrdinalIgnoreCase))
                    {
                        amount = -amount;
                    }

                    var transaction = new ImportedTransaction
                    {
                        Date = date,
                        Amount = amount,
                        Description = reader.GetValue(6)?.ToString() ?? "",
                        Currency = reader.GetValue(8)?.ToString() ?? "RUB",
                        Category = reader.GetValue(10)?.ToString() ?? ""
                    };

                    if (reader.GetValue(5) != null) transaction.AdditionalInfo.Add("CardNumber", reader.GetValue(5).ToString());
                    if (reader.GetValue(9) != null) transaction.AdditionalInfo.Add("Status", reader.GetValue(9).ToString());
                    if (reader.GetValue(11) != null) transaction.AdditionalInfo.Add("MCC", reader.GetValue(11).ToString());

                    result.Add(transaction);
                }
            });

            return result;
        }

        private DateTime GetDateFromCell(object cellValue)
        {
            if (cellValue is DateTime dt) return dt;
            if (cellValue is string str && DateTime.TryParseExact(str, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) return dt;
            return DateTime.MinValue;
        }

        private double GetDoubleFromCell(object cellValue)
        {
            if (cellValue is double d) return d;
            if (cellValue is string str)
            {
                str = str.Replace(" ", "").Replace("\u00A0", "").Replace(".", ",");
                if (double.TryParse(str, NumberStyles.Any, new CultureInfo("ru-RU"), out d)) return d;
            }
            return 0;
        }
    }
}
