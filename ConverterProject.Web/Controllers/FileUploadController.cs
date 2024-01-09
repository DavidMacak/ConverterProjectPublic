using ConverterProject.Web.Models;
using ConverterProject.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ConverterProject.Web.Models.Constants;
using ConverterProject.Web.Models.Types;

namespace ConverterProject.Web.Controllers
{
    public class FileUploadController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string SessionKeyId = "_SessionId";
        private FileManagerService _fileManager;
        private IFileService _fileService;
        private IConfiguration _config;

        public FileUploadController(FileManagerService fileManagerService, IFileService fileService, IConfiguration config)
        {
            _fileManager = fileManagerService;
            _fileService = fileService;
            _config = config;
        }


        public async Task<IActionResult> UploadEmailAsync(IEnumerable<IFormFile> filesToUpload)
        {
            string sessionId = HttpContext.Session.GetString(SessionKeyId);
            log.Info($"Uploading {filesToUpload.Count()} files");
            if (!string.IsNullOrEmpty(sessionId) && CanFilesBeUploaded(sessionId, filesToUpload.Count()))
            {
                foreach (var emailFile in filesToUpload)
                {
                    // TODO: zjistit lepší způsob omezení nahrávání souborů
                    // TODO: vrátit do view zprávu že uživatel překročil limit
                    FileModel fileModel = new FileModel();
                    GetFileType(emailFile, fileModel);
                    if (emailFile.Length < _config.GetValue<int>("MaximumFileSize") && fileModel.FileType != FileType.Unsupported)
                    {
                        CreatingNameForFileModel(sessionId, emailFile, fileModel);
                        try
                        {
                            log.Debug($"Sending file {fileModel.FileType} to BlobService.");
                            await _fileService.UploadFileAsync(emailFile, fileModel, BlobContainer.ToBeConverted);
                            fileModel.StatusType = StatusType.Uploaded;
                            log.Info("Sending file to FileManagerService");
                            _fileManager.AddFile(fileModel);
                        }
                        catch (Exception ex)
                        {
                            // TODO: poslat do view zprávu o chybě
                            fileModel.StatusType = StatusType.Error;
                            fileModel.ErrorMessage = "Při nahrávání se vyskytla chyba.";
                            log.Error(ex);
                        }
                    }
                }
            }
            return RedirectToAction("Index", "Pdf");
        }

        private bool CanFilesBeUploaded(string sessionId, int howManyFilesToUpload)
        {
            log.Debug("Controlling if user reached upload limit.");
            int userFileCount = _fileManager.GetFilesBySessionId(sessionId).Count();
            int userFileCountWithUpload = userFileCount + howManyFilesToUpload;
            int maximumFiles = _config.GetValue<int>("MaximumFilesPerUser");
            if (userFileCountWithUpload > maximumFiles)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static void CreatingNameForFileModel(string sessionId, IFormFile emailFile, FileModel fileModel)
        {
            string filename = Path.GetFileName(emailFile.FileName);
            filename = filename.Replace(" ", "");
            filename = filename.Substring(0, filename.LastIndexOf("."));

            fileModel.SessionId = sessionId;
            fileModel.OriginalFileName = filename;
            fileModel.TemporaryFileName = Guid.NewGuid().ToString();
        }

        private static void GetFileType(IFormFile emailFile, FileModel fileModel)
        {
            if (emailFile.FileName.ToLower().EndsWith(".eml"))
            {
                fileModel.FileType = FileType.EML;
                log.Debug("File is .eml");
            }
            else if (emailFile.FileName.ToLower().EndsWith(".msg"))
            {
                fileModel.FileType = FileType.MSG;
                log.Debug("File is .msg");
            }
            else
            {
                fileModel.FileType = FileType.Unsupported;
                log.Info("File is not supported");
            }
        }
    }
}
