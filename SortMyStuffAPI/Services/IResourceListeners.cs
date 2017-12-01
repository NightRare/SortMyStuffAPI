using SortMyStuffAPI.Models;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Services
{
    public interface IResourceUpdateListener<T>
        where T : EntityResource
    {
        void OnResourceUpdated(T resource);
    }

    public interface IResourceCreateListener<T>
        where T : EntityResource
    {
        void OnResourceCreated(T resource);
    }

    public interface IResourceDeleteListener<T>
        where T : EntityResource
    {
        void OnResourceDeleted(T resource);
    }
}
