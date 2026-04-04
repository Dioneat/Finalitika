namespace Finalitika10.Models
{
    public enum ExportFormat
    {
        Docx,
        Pdf,
        Xml
    }

    public record DocumentRequest(
        string DocumentType, 
        Dictionary<string, string> Data,
        IEnumerable<ExportFormat> RequestedFormats
    );

    public record DocumentResult(
        ExportFormat Format,
        byte[] Content,
        string FileName
    );
}
