using Azure.Storage.Blobs;
using ConverterProject.Web.Models;

namespace ConverterProject.Web.Services
{
    /// <summary>
    /// Container must exist in Azure Storage. Container name must be small letter case. Use BlobContainer.cs constants for names.
    /// </summary>
    public class BlobService : IFileService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _connectionString;

        public BlobService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("BlobStorageAccount");
        }

        /// <summary>
        /// Used for uploading files from Views.
        /// </summary>
        public async Task UploadFileAsync(IFormFile file, FileModel fileModel, string containerName)
        {
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    await UploadFileAsync(stream, fileModel, containerName);
                };
            }
            catch (Exception ex)
            {
                log.Warn(ex);
            }
        }
        /// <summary>
        /// Uploads file to specific container in Azure Blob Storage.
        /// </summary>
        public async Task UploadFileAsync(Stream file, FileModel fileModel, string containerName)
        {
            // TODO: StatusType.Uploaded by se měl měnit tady.
            try
            {
                BlobClient blobClient = new BlobClient(_connectionString, containerName, fileModel.TemporaryFileName);
                log.Info($"Uploading file {fileModel.TemporaryFileName} to Azure Blob Storage.");
                await blobClient.UploadAsync(file);
            }
            catch (Exception ex)
            {
                log.Warn(ex);
            }
        }

        public async Task UploadFileAsync(byte[] file, FileModel fileModel, string containerName)
        {
            try
            {
                using (MemoryStream stream = new())
                {
                    stream.Read(file, 0, file.Length);
                    BlobClient blobClient = new BlobClient(_connectionString, containerName, fileModel.TemporaryFileName);
                    log.Info($"Uploading file {fileModel.TemporaryFileName} to Azure Blob Storage.");
                    await blobClient.UploadAsync(stream);
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex);
            }
        }

        /// <summary>
        /// Downloads specific file from specific container.
        /// </summary>
        public async Task<Stream> DownloadFileAsync(string fileName, string containerName)
        {
            BlobClient blobClient = new(_connectionString, containerName, fileName);
            try
            {
                Stream stream = new MemoryStream();
                log.Info($"Downloading file {fileName} from {containerName}.");
                await blobClient.DownloadToAsync(stream);
                log.Info($"Downloading file {fileName} is successfull.");
                return stream;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        public async Task<byte[]> DownloadFileAsByteArrayAsync(string fileName, string containerName)
        {
            log.Info($"Trying to download file {fileName} from {containerName}.");
            BlobClient blobClient = new(_connectionString, containerName, fileName);
            try
            {
                using (var stream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(stream);
                    byte[] file = stream.ToArray();
                    return file;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes specific file if it exists from specific container.
        /// </summary>
        public async Task DeleteFileAsync(string fileName, string containerName)
        {
            BlobClient blobClient = new(_connectionString, containerName, fileName);
            try
            {
                log.Info($"Deleting file {fileName} from {containerName}");
                var response = await blobClient.DeleteIfExistsAsync();
                // TODO: poslat zpátky info že proběhlo v pořádku
                if (response)
                {
                    log.Info($"File {fileName} was successfuly deleted from {containerName}.");
                }
                else
                {
                    log.Warn($"File {fileName} was not deleted from {containerName} because it was not found.");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        /// <summary>
        /// Called once on startup for every container.
        /// </summary>
        public async Task DeleteAllFilesAsync(string containerName)
        {
            BlobContainerClient containerClient = new(_connectionString, containerName);
            List<string> blobNames = await GetAllFilesInContainerAsync(containerName);
            log.Info($"Trying to delete all files from {containerName}");
            if (blobNames != null && blobNames.Count > 0)
            {
                foreach (string blobName in blobNames)
                {
                    try
                    {
                        log.Debug($"Deleting file {blobName} from {containerName}");
                        await containerClient.DeleteBlobAsync(blobName);
                    }
                    catch (Exception ex)
                    {
                        log.Warn(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Getting all available files in specific container.
        /// </summary>
        public async Task<List<string>> GetAllFilesInContainerAsync(string containerName)
        {
            List<string> blobNames = new List<string>();
            BlobContainerClient containerClient = new(_connectionString, containerName);
            try
            {
                log.Info($"Getting all files from {containerName}");
                await foreach (var blob in containerClient.GetBlobsAsync())
                {
                    blobNames.Add(blob.Name);
                }
                log.Info($"Found {blobNames.Count} files in {containerName}");
            }
            catch (Exception ex)
            {
                log.Warn(ex);
            }
            return blobNames;
        }
    }
}
