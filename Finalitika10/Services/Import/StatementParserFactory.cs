namespace Finalitika10.Services.Import
{
    public interface IStatementParser
    {
        Task<List<ImportedTransaction>> ParseAsync(Stream fileStream);
    }

    public enum BankFormatType
    {
        AlfaBankCsv,
        TinkoffCsv,
        AlfaBankXlsx,
        TinkoffXlsx,
        TinkoffOfx
    }

    public class StatementParserFactory
    {
        public IStatementParser GetParser(BankFormatType formatType)
        {
            return formatType switch
            {
                BankFormatType.AlfaBankCsv => new AlfaBankCsvParser(),
                BankFormatType.TinkoffCsv => new TinkoffCsvParser(),
                BankFormatType.TinkoffOfx => new TinkoffOfxParser(),
                BankFormatType.AlfaBankXlsx => new AlfaBankXlsxParser(), 
                BankFormatType.TinkoffXlsx => new TinkoffXlsxParser(),   
                _ => throw new ArgumentException($"Формат {formatType} пока не поддерживается.")
            };
        }
    }
}
