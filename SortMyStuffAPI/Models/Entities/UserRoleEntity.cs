using System;
using Microsoft.AspNetCore.Identity;

namespace SortMyStuffAPI.Models
{
    public class UserRoleEntity : IdentityRole<string>
    {
        public UserRoleEntity() 
            : base()
        { }

        public UserRoleEntity(string roleName)
            : base(roleName)
        { }
    }
}
