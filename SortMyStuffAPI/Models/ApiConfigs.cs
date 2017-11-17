using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class ApiConfigs
    {
        public bool AllowAnonymous { get; set; }

        public string StorageAppData { get; set; }
        public string StorageUserData { get; set; }
        public string StorageThumbnails { get; set; }
        public string StoragePhotos { get; set; }

        public int PhotoWidth { get; set; }
        public int PhotoHeight { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }
        public int ImageQuality { get; set; }
        public int ImageMaxBytes { get; set; }
        public string ImageFormat { get; set; }

        [LocalResource]
        public string DefaultPhoto { get; set; }
        [LocalResource]
        public string DefaultThumbnail { get; set; }

        public int BearerTokenLifeTimeInMin { get; set; }

        public int BaseDetailLimit { get; set; }
    }
}
