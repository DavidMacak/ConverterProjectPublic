﻿// TODO: zjistit jestli je dobré používat lock ikdyž je serviska singleton
using ConverterProject.Web.Models;
using ConverterProject.Web.Models.Constants;
using ConverterProject.Web.Models.Types;

namespace ConverterProject.Web.Services
{
    public class FileManagerService : IHostedService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PdfQueueService _pdfQueueService;
        private readonly IFileService _blobService;
        private readonly IConfiguration _config;
        private readonly int _timeLimitAfterConversion;
        private readonly int _timeLimitAfterDownload;
        private CancellationToken _cts;
        private List<FileModel> TemporaryFiles { get; set; } = new List<FileModel>();

        public FileManagerService(PdfQueueService pdfQueueService, IFileService blobService, IConfiguration config)
        {
            _pdfQueueService = pdfQueueService;
            _blobService = blobService;
            _config = config;
            _timeLimitAfterConversion = _config.GetValue<int>("TimeLimitAfterConversion");
            _timeLimitAfterDownload = _config.GetValue<int>("TimeLimitAfterDownload");
            Task.Run(() => DeleteAllFilesAsync());
            Task.Run(() => FileManagerLoop());
        }

        /// <summary>
        /// Creates a new file in the download service if it does not exist or replaces the existing one. It is searched and compared by TemporaryFileName.
        /// </summary>
        public void AddFile(FileModel file)
        {
            log.Debug($"Adding file {file.TemporaryFileName}");
            lock (TemporaryFiles)
            {
                bool repeat = true;
                while (repeat)
                {
                    repeat = false;
                    log.Debug("Finding if file already exists.");
                    FileModel? tempFile = TemporaryFiles.FirstOrDefault(f => f.TemporaryFileName == file.TemporaryFileName);

                    if (tempFile == null)
                    {
                        log.Debug("File does not exists.");
                        TemporaryFiles.Add(file);
                        log.Info($"File {file.TemporaryFileName} is added.");
                    }
                    else
                    {
                        // Prevent a disaster in case two identical GUIDs are generated by chance.
                        if (tempFile.SessionId == file.SessionId)
                        {
                            tempFile = file;
                            log.Info("File is replaced.");
                        }
                        else
                        {
                            // This should never occur. But still...
                            log.Fatal($"Two files has same name but different SessionId!");
                            file.TemporaryFileName = Guid.NewGuid().ToString();
                            repeat = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns all files that belong to the user's SessionId.
        /// </summary>
        public List<FileModel> GetFilesBySessionId(string sessionId)
        {
            log.Debug("Getting all files with matching SessionId.");
            lock (TemporaryFiles)
            {
                List<FileModel> userConvertedFiles = new();
                userConvertedFiles = TemporaryFiles.Where(file => file.SessionId == sessionId).ToList();
                log.Info($"Returning {userConvertedFiles.Count()} files with matching SessionID.");
                return userConvertedFiles;
            }
        }

        /// <summary>
        /// Returns specific file.
        /// </summary>
        public FileModel GetFile(string sessionId, string tempName)
        {
            log.Debug("Getting specific file for download.");
            lock (TemporaryFiles)
            {
                FileModel file = TemporaryFiles.FirstOrDefault(file => file.SessionId == sessionId && file.TemporaryFileName == tempName);
                if (file != null)
                {
                    log.Info("Returning file.");
                    return file;
                }
                else
                {
                    file = new()
                    {
                        StatusType = StatusType.Error,
                        ErrorMessage = "File not found!"
                    };
                    log.Info("File does not exist.");
                    return file;
                }
            }
        }

        /// <summary>
        /// Endless loop that checks files and decides what to do - send to Queue for conversion or delete.
        /// </summary>
        private async Task FileManagerLoop()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    if (TemporaryFiles.Count > 0)
                    {
                        foreach (var file in TemporaryFiles)
                        {
                            if (file != null && file.StatusType != StatusType.InQueue)
                            {
                                if (file.StatusType == StatusType.Uploaded)
                                {
                                    log.Debug($"Sending file {file.TemporaryFileName} to QueueService");
                                    _pdfQueueService.EnqueueFile(file);
                                }
                                else if (file.StatusType == StatusType.Error)
                                {
                                    log.Debug($"Deleting file {file.TemporaryFileName} because of error {file.ErrorMessage}");
                                    await DeleteFileAsync(file);
                                }
                                else if (file.StatusType == StatusType.Downloaded)
                                {
                                    // TODO: hodit to do jiné smyčky která to bude kontrolovat co 5 minut?
                                    TimeSpan span = DateTime.Now - file.TimeOfLastInteraction;
                                    if (span.Minutes >= _timeLimitAfterDownload)
                                    {
                                        log.Info($"File {file.TemporaryFileName} has reached time limit. It will be deleted");
                                        await DeleteFileAsync(file);
                                    }
                                }
                            }
                            else if (file != null && file.StatusType == StatusType.Converted)
                            {
                                TimeSpan span = DateTime.Now - file.TimeOfLastInteraction;
                                if (span.Minutes >= _timeLimitAfterConversion)
                                {
                                    log.Info($"File {file.TemporaryFileName} has reached time limit. It will be deleted");
                                    await DeleteFileAsync(file);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
                await Task.Delay(1000, _cts);
            }
        }

        /// <summary>
        /// Deletes all file occurences in ToBeConverted and Converted containers.
        /// </summary>
        public async Task DeleteFileAsync(FileModel file)
        {
            try
            {
                if (file.FileType == FileType.PDF)
                {
                    await _blobService.DeleteFileAsync(file.TemporaryFileName, BlobContainer.Converted);
                }
                else
                {
                    await _blobService.DeleteFileAsync(file.TemporaryFileName, BlobContainer.ToBeConverted);
                }
                log.Debug($"Removing file {file.TemporaryFileName} from TemporaryFiles list.");
                lock (TemporaryFiles)
                {
                    TemporaryFiles.Remove(file);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        /// <summary>
        /// This is called once at the start of application. Deletes all un/converted files.
        /// </summary>
        private async Task DeleteAllFilesAsync()
        {
            // TODO: metoda pro všechny kontejnery?
            await _blobService.DeleteAllFilesAsync(BlobContainer.ToBeConverted);
            await _blobService.DeleteAllFilesAsync(BlobContainer.Converted);
        }

        // Preparations for future features. For example: deleting all files in storage when app is needed to shut down.
        public Task StartAsync(CancellationToken cancellationToken)
        {
            log.Info("Starting DownloadService");
            _cts = cancellationToken;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            log.Info("Stopping DownloadService");
            _cts = cancellationToken;
            return Task.CompletedTask;
        }
    }
}
