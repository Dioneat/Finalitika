using System.Globalization;
using System.Xml.Linq;

namespace Finalitika10.Services.Import
{
    public class TinkoffOfxParser : IStatementParser
    {
        public async Task<List<ImportedTransaction>> ParseAsync(Stream fileStream)
        {
            var result = new List<ImportedTransaction>();

            using var reader = new StreamReader(fileStream);
            var content = await reader.ReadToEndAsync();

            var ofxStartIndex = content.IndexOf("<OFX>");
            if (ofxStartIndex < 0) throw new Exception("Файл не является валидным OFX (не найден тег <OFX>)");

            var xmlContent = content.Substring(ofxStartIndex);

            var doc = XDocument.Parse(xmlContent);

            var transactions = doc.Descendants("STMTTRN");

            foreach (var trn in transactions)
            {
                var dateStr = trn.Element("DTPOSTED")?.Value;
                var amountStr = trn.Element("TRNAMT")?.Value;
                var name = trn.Element("NAME")?.Value ?? "";
                var memo = trn.Element("MEMO")?.Value ?? "";
                var fitId = trn.Element("FITID")?.Value ?? "";

                if (TryParseOfxDate(dateStr, out DateTime date) &&
                    double.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double amount))
                {
                    var imported = new ImportedTransaction
                    {
                        Date = date,
                        Amount = amount,
                        Description = name,
                        Category = memo,
                        Currency = "RUB"
                    };

                    if (!string.IsNullOrEmpty(fitId)) imported.AdditionalInfo.Add("BankTransactionId", fitId);

                    var accountId = trn.Ancestors("STMTRS").Descendants("ACCTID").FirstOrDefault()?.Value;
                    if (!string.IsNullOrEmpty(accountId)) imported.AdditionalInfo.Add("BankAccountId", accountId);

                    result.Add(imported);
                }
            }

            return result;
        }

        private bool TryParseOfxDate(string ofxDate, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(ofxDate) || ofxDate.Length < 14) return false;

            var cleanDateStr = ofxDate.Substring(0, 14);

            return DateTime.TryParseExact(
                cleanDateStr,
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result);
        }
    }
}