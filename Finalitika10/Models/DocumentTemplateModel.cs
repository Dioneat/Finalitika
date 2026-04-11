namespace Finalitika10.Models
{
    public class DocumentTemplateModel
    {
        public string DocumentType { get; set; }
        public string Title { get; set; }

        public string Description { get; set; } = "";
        public string Icon { get; set; } = "📄";
        public string IconBgColor { get; set; } = "#F4F6F8"; 
        public string Tag { get; set; } = ""; 
        public string TagColor { get; set; } = "#E74C3C";

        public bool HasTag => !string.IsNullOrEmpty(Tag);
    }
}