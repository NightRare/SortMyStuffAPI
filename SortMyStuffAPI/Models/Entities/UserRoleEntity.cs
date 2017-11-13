using System;
using Microsoft.AspNetCore.Identity;

namespace SortMyStuffAPI.Models
{
    public class UserRoleEntity : IdentityRole<string>, IEntity
    {
        public UserRoleEntity() 
            : base()
        { }

        public UserRoleEntity(string roleName)
            : base(roleName)
        { }
    }
}
