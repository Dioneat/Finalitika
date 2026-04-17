using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Finalitika10.Models
{
    public sealed class ChatMessage
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public bool IsUser { get; init; }
        public string Text { get; init; } = string.Empty;
        public string HtmlText { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }

        public LayoutOptions MessageAlignment => IsUser ? LayoutOptions.End : LayoutOptions.Start;
        public bool ShowAiAvatar => !IsUser;

        public Color BackgroundColor => IsUser
            ? Color.FromArgb("#3498DB")
            : Colors.White;

        public Color TextColor => IsUser
            ? Colors.White
            : Color.FromArgb("#2C3E50");

        public Color TimeColor => IsUser
            ? Color.FromArgb("#D6EAF8")
            : Color.FromArgb("#95A5A6");

        public CornerRadius BubbleRadius => IsUser
            ? new CornerRadius(20, 20, 4, 20)
            : new CornerRadius(20, 20, 20, 4);

        public string TimeString => Timestamp.ToString("HH:mm");
    }
}