using ConverterProject.Web.Models;
using ConverterProject.Web.Models.Constants;
using ConverterProject.Web.Models.Types;
using ConverterProject.Web.Services.Providers;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Layout;
using MimeKit;
using MsgReader.Outlook;

namespace ConverterProject.Web.Services
{
    public class PdfConverterService : IHostedService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IFileService _fileService;
        private PdfConverterProperties _converterProperties;

        public PdfConverterService(IFileService fileService, PdfConverterProperties converterProperties)
        {
            _fileService = fileService;
            _converterProperties = converterProperties;
        }

        /// <summary>
        /// Takes FilesToConvertModel. Converts it to PDF.
        /// </summary>
        public async Task ToPdfAsync(FileModel file)
        {
            log.Info($"Converting {file.OriginalFileName}.");
            if (file.FileType == FileType.EML)
            {
                log.Info("File is of EML type.");
                await ConvertEmlToPdfAsync(file);
            }
            else if (file.FileType == FileType.MSG)
            {
                log.Info("File is of MSG type.");
                await ConvertMsgToPdfAsync(file);
            }
            // TODO: pokud neproběhne tak false

            // Gets some RAM back
            GC.Collect();
        }

        private async Task ConvertMsgToPdfAsync(FileModel file)
        {
            log.Debug($"Converting .msg to Pdf.");
            string htmlBody = await ConvertMsgToHtmlAsync(file);
            await ConvertHtmltoPdfAsync(htmlBody, file);
        }

        private async Task ConvertEmlToPdfAsync(FileModel file)
        {
            log.Info($"Converting .eml to Pdf.");
            string html = await ConvertEmlToHtmlAsync(file);
            await ConvertHtmltoPdfAsync(html, file);
        }

        private async Task<string> ConvertMsgToHtmlAsync(FileModel file)
        {
            log.Info("Converting .msg file to HTML.");
            try
            {
                using (Stream stream = await _fileService.DownloadFileAsync(file.TemporaryFileName, BlobContainer.ToBeConverted))
                {
                    using (var msg = new Storage.Message(stream))
                    {
                        log.Info("HTML conversion successful.");
                        log.Info("Deleting .msg file.");
                        await _fileService.DeleteFileAsync(file.TemporaryFileName, BlobContainer.ToBeConverted);
                        return msg.BodyHtml;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                file.StatusType = StatusType.Error;
                file.ErrorMessage = ex.Message;
                throw;
            }
        }

    private async Task<string> ConvertEmlToHtmlAsync(FileModel file)
    {
        log.Info("Converting .eml file to HTML.");
        try
        {
            using (Stream stream = await _fileService.DownloadFileAsync(file.TemporaryFileName, BlobContainer.ToBeConverted))
            {
                stream.Position = 0;
                using (MimeMessage eml = MimeMessage.Load(stream))
                {
                    log.Info("HTML conversion successful.");
                    log.Info("Deleting .eml file.");
                    await _fileService.DeleteFileAsync(file.TemporaryFileName, BlobContainer.ToBeConverted);
                    return eml.HtmlBody;
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
            file.StatusType = StatusType.Error;
            file.ErrorMessage = ex.Message;
            throw;
        }
    }

    private async Task ConvertHtmltoPdfAsync(string htmlBody, FileModel file)
    {
        try
        {
            using (Stream stream = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(stream);
                log.Info("Converting HTML to PDF.");
                PdfDocument pdf = new PdfDocument(writer);
                writer.SetCloseStream(false);
                Document document = HtmlConverter.ConvertToDocument(htmlBody, writer, _converterProperties.converterProperties);
                document.Close();

                stream.Position = 0;
                file.FileType = FileType.PDF;
                log.Info("Conversion successful.");
                log.Info($"Uploading file converted {file.TemporaryFileName} to blob storage.");
                await _fileService.UploadFileAsync(stream, file, BlobContainer.Converted);
                file.StatusType = StatusType.Converted;
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
            file.StatusType = StatusType.Error;
            file.ErrorMessage = ex.Message;
            throw;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
}


