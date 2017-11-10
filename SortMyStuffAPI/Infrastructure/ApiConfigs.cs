namespace SortMyStuffAPI.Infrastructure
{
    public class ApiConfigs
    {
        public string RootAssetId { get; set; }

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
    }
}
