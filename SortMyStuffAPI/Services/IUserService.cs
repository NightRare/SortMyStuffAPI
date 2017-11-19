using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Services
{
    public interface IUserService
    {
        Task<User> GetUserResourceAsync(ClaimsPrincipal user);

        //Task<User> GetUserResourceByIdAsync(string id);

        //Task<User> GetUserResourceByEmailAsync(string email);

        Task<UserEntity> GetUserAsync(ClaimsPrincipal user);

        Task<UserEntity> GetUserByIdAsync(string id);

        Task<UserEntity> GetUserByEmailAsync(string email);

        Task<UserEntity> GetUserByNameAsync(string userName);

        Task<string> GetUserIdAsync(ClaimsPrincipal user);

        Task<IResult> UpdateAsync(UserEntity user);

        Task<bool> CheckPasswordAsync(UserEntity user, string password);
    }
}
