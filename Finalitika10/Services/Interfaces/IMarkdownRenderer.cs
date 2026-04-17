namespace Finalitika10.Services
{
    public interface IMarkdownRenderer
    {
        string ToHtml(string? markdownText);
    }
}