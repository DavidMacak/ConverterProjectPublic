using ConverterProject.Web.Models;

namespace ConverterProject.Web.Services
{
    public interface IFileService
    {
        Task DeleteFileAsync(string fileName, string destinationName);
        Task DeleteAllFilesAsync(string destinationName);
        Task<Stream> DownloadFileAsync(string fileName, string destinationName);
        Task<List<string>> GetAllFilesInContainerAsync(string destinationName);
        Task UploadFileAsync(IFormFile file, FileModel fileModel, string destinationName);
        Task UploadFileAsync(Stream file, FileModel fileModel, string destinationName);

    }
}