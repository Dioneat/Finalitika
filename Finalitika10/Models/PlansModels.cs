namespace Finalitika10.Models
{

    public class LifeScenario
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ActionText { get; set; } 
        public string ActionColor { get; set; } 
    }



    public class ProjectNote
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "Новая заметка";
        public string MarkdownText { get; set; } = "";
        public string PreviewText { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Date => CreatedAt.ToString("dd MMM yyyy");
        public string ColorHex { get; set; } = "#F8F9FA";
    }
}
