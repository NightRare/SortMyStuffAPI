using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Utils;
using System;

namespace SortMyStuffAPI.Utils
{
    // TODO: Class to be removed in production

    /// <summary>
    /// This tool class is a hacking way to bypass the userId check in 
    /// IDataService and IFileService. It should be removed in production
    /// stage.
    /// </summary>
    public static class ServicesAuthHelper
    {
        public readonly static string DeveloperUid = 
            Environment.GetEnvironmentVariable(ApiStrings.EnvDeveloperUid);

        /// <summary>
        /// Checks whether the given userId is same as the Developer Uid.
        /// Returns false if no Developer Uid found in the environment variables.
        /// </summary>
        /// 
        /// <param name="userId">
        /// the userId
        /// </param>
        /// 
        /// <returns>
        /// true if the given user id is same as Developer Uid.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// If userId is null
        /// </exception>
        /// 
        public static bool IsDeveloper(string userId)
        {
            if (userId == null)
                throw new ArgumentNullException("The userId cannot be null");

            if (DeveloperUid == null)
                return false;

            return DeveloperUid.Equals(userId);
        }
    }
}
