using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SortMyStuffAPI.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Infrastructure;
using System.Security.Claims;
using SortMyStuffAPI.Utils;
using System.Reflection;

namespace SortMyStuffAPI.Services
{
    public class DefaultUserDataService :
        DefaultDataService<User, UserEntity>,
        IUserDataService
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<UserRoleEntity> _roleManager;

        public DefaultUserDataService(
            SortMyStuffContext dbContext,
            IOptions<ApiConfigs> apiConfigs,
            UserManager<UserEntity> userManager,
            RoleManager<UserRoleEntity> roleManager)
            : base(dbContext, apiConfigs)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        #region IDataService METHODS

        public async Task<User> GetResourceAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            // always pass in developer id
            return await GetOneResourceAsync(
                DbContext.Users,
                ServicesAuthHelper.DeveloperUid,
                id,
                ct);
        }

        public async Task<PagedResults<User>> GetResouceCollectionAsync(
            string userId,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<User, UserEntity> sortOptions = null,
            SearchOptions<User, UserEntity> searchOptions = null)
        {
            // ignore userId
            return await GetOneTypeResourcesAsync(
                ServicesAuthHelper.DeveloperUid,
                DbContext.Users,
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        public async Task<(bool Succeeded, string Error)> AddResourceAsync(
            string userId,
            User resource,
            CancellationToken ct)
        {
            var record = await _userManager.FindByNameAsync(resource.UserName);
            if (record != null)
                return (false, "Existing UserName.");

            record = await _userManager.FindByEmailAsync(resource.Email);
            if (record != null)
                return (false, "Existing Email.");

            var entity = Mapper.Map<User, UserEntity>(resource);
            var result = await _userManager.CreateAsync(entity, resource.Password);
            if (!result.Succeeded)
            {
                var defaultError = result.Errors.FirstOrDefault()?.Description;
                return (false, defaultError);
            }
            return (true, null);
        }

        public async Task<(bool Succeeded, string Error)> AddResourceCollectionAsync(
            string userId,
            ICollection<User> resources,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool Succeeded, string Error)> UpdateResourceAsync(
            string userId,
            User resource,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }


        public async Task<(bool Succeeded, string Error)> DeleteResourceAsync(
            string userId,
            string resourceId,
            bool delDependents,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CheckScopedUniquenessAsync(
            string userId,
            PropertyInfo property,
            User resource,
            Scope scope,
            CancellationToken ct)
        {
            return await Task.Run(() =>
                CheckResourceScopedUniqueness(
                    userId,
                    property,
                    resource,
                    scope,
                    DbContext.Users), ct);
        }

        #endregion


        #region IUserDataService METHODS

        public async Task<User> GetUserAsync(ClaimsPrincipal user)
        {
            return Mapper.Map<UserEntity, User>(await GetUserEntityAsync(user));
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return Mapper.Map<UserEntity, User>(await GetUserEntityByIdAsync(id));
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return Mapper.Map<UserEntity, User>(await GetUserEntityByEmailAsync(email));
        }

        public async Task<UserEntity> GetUserEntityAsync(ClaimsPrincipal user)
        {
            return await _userManager.GetUserAsync(user);
        }

        public async Task<UserEntity> GetUserEntityByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<UserEntity> GetUserEntityByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<string> GetUserIdAsync(ClaimsPrincipal user)
        {
            return await Task.Run(() => _userManager.GetUserId(user));
        }

        #endregion
    }
}