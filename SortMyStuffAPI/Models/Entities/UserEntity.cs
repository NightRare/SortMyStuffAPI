using System;
using Microsoft.AspNetCore.Identity;

namespace SortMyStuffAPI.Models
{
    public class UserEntity : IdentityUser<string>, IEntity
    {
        public AuthProvider Provider { get; set; }
            = AuthProvider.Native;

        public DateTimeOffset CreateTimestamp { get; set; }
    }
}
