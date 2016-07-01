using Discord;
using System;

namespace Discord_Bot
{
    public enum RoleType
    {
        User,
        Admin,
        Other
    }

    public class UserRole
    {
        public ulong role { get; private set; }
        public RoleType roleType { get; private set; }

        public UserRole(ulong role, RoleType roleType)
        {
            this.role = role; this.roleType = roleType;
        }
    }
}
