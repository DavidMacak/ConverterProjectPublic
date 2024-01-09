// TODO: Preparation for maybe new feature (limit how many files can session have).
namespace ConverterProject.Web.Models
{
    public class UserUploadModel
    {
        public string SessionId { get; set; } = string.Empty;
        public List<FileModel> Files { get; set; } = new List<FileModel>();
    }
}
