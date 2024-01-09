using ConverterProject.Web.Models;
using ConverterProject.Web.Models.Types;

namespace ConverterProject.Web.Services
{
    public class PdfQueueService : IHostedService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private PdfConverterService _converterService;
        private List<FileModel> _filesInQueue;
        private CancellationToken _cts;
        public PdfQueueService(PdfConverterService converterService)
        {
            _converterService = converterService;
            _filesInQueue = new();
            log.Info("Starting queue");
            Task.Run(() => ProcessQueueAsync());

        }

        public void EnqueueFile(FileModel file)
        {
            lock (_filesInQueue)
            {
                log.Info($"Enqueuing file {file.TemporaryFileName}");
                file.StatusType = StatusType.InQueue;
                _filesInQueue.Add(file);
            }
        }

        public FileModel DequeueFile()
        {
            lock (_filesInQueue)
            {
                if (_filesInQueue.Count > 0)
                {
                    FileModel file = _filesInQueue[0];
                    log.Info($"Dequeuing file {file.TemporaryFileName}");
                    _filesInQueue.RemoveAt(0);
                    return file;
                }
                else
                {
                    // TODO: udělat lepší řešení
                    return null;
                }
            }
        }

        public async Task ProcessQueueAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    FileModel file = DequeueFile();
                    if (file != null)
                    {
                        // TODO tasky - odbouchnot a jít pryč, nečekat na dokončení - více konverzí zároveň - sledovat RAM/CPU
                        //log.Debug("Changing StatusType to InConversion");
                        log.Info("Sending file to conversion");
                        file.StatusType = StatusType.InConversion;
                        await _converterService.ToPdfAsync(file);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
                await Task.Delay(1000, _cts);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            log.Info("Starting QueueService");
            Task.Run(() => ProcessQueueAsync());
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            log.Info("Stopping QueueService");
            _cts = cancellationToken;
            return Task.CompletedTask;
        }
    }
}
