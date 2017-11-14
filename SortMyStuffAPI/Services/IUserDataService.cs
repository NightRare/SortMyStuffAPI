using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public interface IUserDataService : IDataService<User, UserEntity>
    {
        Task<(bool Succeeded, string Error)> CreateUserAsync(
            User user, CancellationToken ct);

        Task<User> GetUserAsync(ClaimsPrincipal user);

        Task<User> GetUserByIdAsync(string id);

        Task<User> GetUserByEmailAsync(string email);

        Task<UserEntity> GetUserEntityAsync(ClaimsPrincipal user);

        Task<UserEntity> GetUserEntityByIdAsync(string id);

        Task<UserEntity> GetUserEntityByEmailAsync(string email);
        
        Task<string> GetUserIdAsync(ClaimsPrincipal user);
    }
}
