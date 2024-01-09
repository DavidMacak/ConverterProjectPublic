using ConverterProject.Web.Models;

namespace ConverterProject.Web.Services
{
    public class LocalFileService : IFileService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _path;

        public LocalFileService(IConfiguration config)
        {
            _path = config.GetValue<string>("LocalStoragePath");
        }
        public Task DeleteAllFilesAsync(string destinationName)
        {
            try
            {
                string path = Path.Combine(_path, destinationName);
                string[] files = Directory.GetFiles(path);

                if (files.Length > 0)
                {
                    log.Info($"Deletig all files from local {destinationName}");
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        public Task DeleteFileAsync(string fileName, string destinationName)
        {
            try
            {
                string filePath = Path.Combine(_path, destinationName, fileName);
                log.Info($"Deleting {fileName} from local {destinationName}.");
                File.Delete(filePath);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileName, string destinationName)
        {
            try
            {
                string filePath = Path.Combine(_path, destinationName, fileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    log.Info($"Loading {fileName} from local {destinationName}.");
                    await fs.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        public async Task<List<string>> GetAllFilesInContainerAsync(string destinationName)
        {
            try
            {
                string path = Path.Combine(_path, destinationName);
                log.Info($"Getting all files from local {destinationName}.");
                string[] files = await Task.Run(() => Directory.GetFiles(path));
                return files.ToList();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        public async Task UploadFileAsync(IFormFile file, FileModel fileModel, string destinationName)
        {
            string filePath = Path.Combine(_path, destinationName, fileModel.TemporaryFileName);

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    log.Info($"Saving {fileModel.TemporaryFileName} to local {destinationName}.");
                    await file.CopyToAsync(fs);
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex);
                throw;
            }
        }

        public async Task UploadFileAsync(Stream file, FileModel fileModel, string destinationName)
        {
            try
            {
                string filePath = Path.Combine(_path, destinationName, fileModel.TemporaryFileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    log.Info($"Saving {fileModel.TemporaryFileName} to local {destinationName}.");
                    await file.CopyToAsync(fs);
                }

            }
            catch (Exception ex)
            {
                log.Warn(ex);
            }
        }
    }
}
