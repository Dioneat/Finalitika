using Markdig;

namespace Finalitika10.Services
{
    public sealed class MarkdownRenderer : IMarkdownRenderer
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        public string ToHtml(string? markdownText)
        {
            if (string.IsNullOrWhiteSpace(markdownText))
                return string.Empty;

            var html = Markdown.ToHtml(markdownText, Pipeline);

            html = html.Replace("<p>", string.Empty)
                       .Replace("</p>", "<br>");

            return html.TrimEnd('<', 'b', 'r', '>');
        }
    }
}
