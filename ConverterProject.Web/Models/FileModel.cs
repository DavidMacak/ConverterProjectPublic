using ConverterProject.Web.Models.Types;

namespace ConverterProject.Web.Models
{
    public class FileModel
    {
        private StatusType statusType = StatusType.Init;

        public string SessionId { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string TemporaryFileName { get; set; } = string.Empty;
        public FileType FileType { get; set; } = FileType.Unknown;
        /// <summary>
        /// When changed, it automatically changes TimeOfLastInteraction to current time. Doesn't change when DownloadCount is not 0.
        /// </summary>
        public StatusType StatusType
        {
            get => statusType;
            set
            {
                statusType = value;
                // Changes time every time StatusType is changed until first download. Needed for automatically deleting files based on time.
                if (DownloadCount == 0)
                {
                    TimeOfLastInteraction = DateTime.Now;
                }
            }
        }
        public string ErrorMessage { get; set; } = string.Empty;
        /// <summary>
        /// Time when StatusType was changed. Also time of first file download.
        /// </summary>
        public DateTime TimeOfLastInteraction { get; set; } = DateTime.Now;
        public int DownloadCount { get; set; } = 0;
    }
}
