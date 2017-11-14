using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Services
{
    public interface IPhotoFileService : IFileService
    {
        Task<Stream> DownloadPhoto(
            string userId,
            string id, 
            CancellationToken ct);

        Task<bool> UploadPhoto(
            string userId,
            string id,
            Stream photo,
            CancellationToken ct);

        Task<bool> DeletePhoto(
            string userId,
            string id, 
            CancellationToken ct);
    }
}
