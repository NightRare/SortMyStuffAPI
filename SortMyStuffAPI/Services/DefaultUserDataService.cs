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
using Microsoft.EntityFrameworkCore;

namespace SortMyStuffAPI.Services
{
    public class DefaultUserDataService :
        DefaultDataService<User, UserEntity>,
        IUserDataService,
        IUserService
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


        #region IUserService METHODS

        public async Task<User> GetUserResourceAsync(ClaimsPrincipal user)
        {
            return Mapper.Map<UserEntity, User>(await GetUserAsync(user));
        }

        public async Task<User> GetUserResourceByIdAsync(string id)
        {
            return Mapper.Map<UserEntity, User>(await GetUserByIdAsync(id));
        }

        public async Task<User> GetUserResourceByEmailAsync(string email)
        {
            return Mapper.Map<UserEntity, User>(await GetUserByEmailAsync(email));
        }

        public async Task<UserEntity> GetUserAsync(ClaimsPrincipal userClaims)
        {
            return await _userManager.GetUserAsync(userClaims);
        }

        public async Task<UserEntity> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<UserEntity> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<string> GetUserIdAsync(ClaimsPrincipal user)
        {
            return await Task.Run(() => _userManager.GetUserId(user));
        }

        public async Task<IResult> UpdateAsync(UserEntity user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ?
                ResultFactory.Success() :
                ResultFactory.Failure(
                    $"Update user {user.Id} failed.", 
                    result.Errors);
        }

        public async Task<bool> CheckPasswordAsync(UserEntity user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<UserEntity> GetUserByNameAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        #endregion
    }
}