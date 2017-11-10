using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Services
{
    public interface IThumbnailFileService : IFileService
    {
        Task<Stream> DownloadThumbnail(string id, CancellationToken ct);
    }
}
