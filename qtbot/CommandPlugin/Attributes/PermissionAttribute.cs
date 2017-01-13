using System;
using System.Collections.Generic;
using System.Text;

namespace qtbot.CommandPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    class PermissionAttribute : Attribute
    {
        public Permission permission;

        public PermissionAttribute(Permission permission)
        {
            this.permission = permission;
        }
    }

    public enum Permission
    {
        USER = 0,
        ADMIN = 500,
        OWNER = 5000,
        BOTOWNER = 50000
    }
}
