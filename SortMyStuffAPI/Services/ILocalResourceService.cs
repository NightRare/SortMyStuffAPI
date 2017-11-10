using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Services
{
    public interface ILocalResourceService
    {
        Stream GetResource(LocalResources resTag);

        Task<Stream> GetResourceAsync(LocalResources resTag, CancellationToken ct);
    }
}
