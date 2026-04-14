using Microsoft.Maui;
using System;

namespace Finalitika10.Models
{
    public class ChatMessage
    {
        public string Text { get; set; }
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; }

        public string TimeString => Timestamp.ToString("HH:mm");

        // --- ВЫЧИСЛЯЕМЫЕ СВОЙСТВА ДЛЯ КРАСИВОГО XAML ---

        // Выравнивание: Юзер справа, ИИ слева
        public LayoutOptions MessageAlignment => IsUser ? LayoutOptions.End : LayoutOptions.Start;

        // Цвета пузырей
        public string BackgroundColor => IsUser ? "#3498DB" : "#FFFFFF"; // Синий для юзера, белый для ИИ
        public string TextColor => IsUser ? "#FFFFFF" : "#2C3E50";
        public string TimeColor => IsUser ? "#A9CCE3" : "#BDC3C7";

        // Хвостики пузырей: Острый угол внизу справа для юзера, внизу слева для ИИ
        public CornerRadius BubbleRadius => IsUser
            ? new CornerRadius(20, 20, 4, 20)
            : new CornerRadius(20, 20, 20, 4);

        public bool ShowAiAvatar => !IsUser;
    }
}