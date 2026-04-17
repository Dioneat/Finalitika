using Markdig;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace Finalitika10.Converters
{
    public class MarkdownToHtmlConverter : IValueConverter
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string markdownText && !string.IsNullOrWhiteSpace(markdownText))
            {
                var html = Markdown.ToHtml(markdownText, Pipeline);

                html = html.Replace("<p>", "").Replace("</p>", "<br>");
                return html.TrimEnd('<', 'b', 'r', '>');
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}