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
        Task<Stream> DownloadPhoto(string id, CancellationToken ct);

        Task<Stream> DownloadDefaultPhoto(CancellationToken ct);

        Task<bool> UploadPhoto(
            string id,
            Stream photo,
            CancellationToken ct);

        Task<bool> DeletePhoto(string id, CancellationToken ct);
    }
}
