// TODO: Preparation for maybe new feature (limit how many files can session have).
namespace ConverterProject.Web.Models
{
    public class SessionModel
    {
        public string SessionId { get; set; } = string.Empty;
        public List<FileModel> Files { get; set; } = new();
        public DateTime TimeOfLastInteraction { get; set; } = DateTime.Now;
    }
}
