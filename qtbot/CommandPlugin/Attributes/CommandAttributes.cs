using System;
using System.Collections.Generic;
using System.Text;

namespace qtbot.CommandPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    class CommandAttributes : Attribute
    {
        public CommandType commandType { get; }
        public string commandName { get; }

        public CommandAttributes(string commandName, CommandType commandType = CommandType.User)
        {
            this.commandName = commandName;
            this.commandType = commandType;
        }
    }

    public enum CommandType
    {
        User,
        Admin
    }
}
