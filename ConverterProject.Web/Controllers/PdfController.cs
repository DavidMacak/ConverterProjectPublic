using ConverterProject.Web.Models;
using ConverterProject.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ConverterProject.Web.Models.Constants;
using ConverterProject.Web.Models.Types;

namespace ConverterProject.Web.Controllers
{
    public class PdfController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly FileManagerService _fileManager;
        private readonly IFileService _blobService;
        public const string SessionKeyId = "_SessionId";

        public PdfController(FileManagerService fileManager, IFileService blobService)
        {
            _fileManager = fileManager;
            _blobService = blobService;
        }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyId)))
            {
                HttpContext.Session.SetString(SessionKeyId, Guid.NewGuid().ToString());
            }
            List<FileModel> files = _fileManager.GetFilesBySessionId(HttpContext.Session.GetString(SessionKeyId));
            return View(files);
        }

        [HttpGet]
        public IActionResult GetFiles()
        {
            List<FileModel> files;
            files = _fileManager.GetFilesBySessionId(HttpContext.Session.GetString(SessionKeyId));
            return Json(files);
        }

        // TODO: hodit tohle do DownloadControlleru
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(string filename)
        {
            log.Info($"User want to download file {filename}");
            string sessionId = HttpContext.Session.GetString(SessionKeyId);
            FileModel file;
            if(!string.IsNullOrEmpty(sessionId) )
            {
                file = _fileManager.GetFile(sessionId, filename);
            }
            else
            {
                log.Warn("There was no SessionId.");
                return RedirectToAction("Index");
            }

            // TODO: poslat do view zprávu o chybě
            if (file.StatusType != StatusType.Error)
            {
                try
                {
                    Stream stream = await _blobService.DownloadFileAsync(filename, BlobContainer.Converted);
                    log.Info($"File {file.TemporaryFileName} is downloaded from blob storage.");

                    stream.Seek(0, SeekOrigin.Begin);
                    file.StatusType = StatusType.Downloaded;
                    file.DownloadCount++;

                    log.Debug($"File: {file.TemporaryFileName} DownloadCount: {file.DownloadCount}, TimeOfLastInteraction: {file.TimeOfLastInteraction}");
                    log.Info($"Sending file {file.TemporaryFileName} to user");

                    return File(stream, "application/pdf", $"{file.OriginalFileName}.pdf");
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteFile(string filename)
        {
            log.Info($"User want to delete file {filename}");
            string? sessionId = HttpContext.Session.GetString(SessionKeyId);
            if (!string.IsNullOrEmpty(sessionId))
            {
                FileModel file = _fileManager.GetFile(sessionId, filename);
                // TODO: poslat do view zprávu o chybě
                if (file.StatusType != StatusType.Error)
                {
                    await _fileManager.DeleteFileAsync(file);
                }
            }
            return RedirectToAction("Index");
        }
    }
}
