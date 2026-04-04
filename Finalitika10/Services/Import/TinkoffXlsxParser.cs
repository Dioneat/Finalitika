using ExcelDataReader;
using System.Globalization;

namespace Finalitika10.Services.Import
{
    public class TinkoffXlsxParser : IStatementParser
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
                    if (reader.GetValue(0) == null || reader.GetValue(4) == null) continue;

                    DateTime date = GetDateFromCell(reader.GetValue(0));
                    if (date == DateTime.MinValue) continue;

                    double amount = GetDoubleFromCell(reader.GetValue(4));

                    var transaction = new ImportedTransaction
                    {
                        Date = date,
                        Amount = amount,
                        Currency = reader.GetString(5) ?? "RUB",
                        Category = reader.GetString(9) ?? "",
                        Description = reader.GetString(11) ?? ""
                    };

                    if (reader.GetValue(2) != null) transaction.AdditionalInfo.Add("CardNumber", reader.GetValue(2).ToString());
                    if (reader.GetValue(3) != null) transaction.AdditionalInfo.Add("Status", reader.GetValue(3).ToString());
                    if (reader.GetValue(10) != null) transaction.AdditionalInfo.Add("MCC", reader.GetValue(10).ToString());

                    result.Add(transaction);
                }
            });

            return result;
        }

        private DateTime GetDateFromCell(object cellValue)
        {
            if (cellValue is DateTime dt) return dt;
            if (cellValue is string strDate && DateTime.TryParseExact(strDate, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) return dt;
            if (cellValue is string strDate2 && DateTime.TryParseExact(strDate2, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) return dt;
            return DateTime.MinValue;
        }

        private double GetDoubleFromCell(object cellValue)
        {
            if (cellValue is double d) return d;
            if (cellValue is string str)
            {
                str = str.Replace(" ", "").Replace("\u00A0", "");
                if (double.TryParse(str, NumberStyles.Any, new CultureInfo("ru-RU"), out d)) return d;
            }
            return 0;
        }
    }
}