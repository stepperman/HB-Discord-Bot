using System;
using System.Collections.Generic;
using System.Text;

namespace qtbot.CommandPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    class CommandAttribute : Attribute
    {
        public CommandType commandType { get; }
        public string commandName { get; }
        public string[] commandAlias { get; }

        public CommandAttribute(string commandName, CommandType commandType = CommandType.User)
        {
            this.commandName = commandName;
            this.commandType = commandType;
        }

        public CommandAttribute(string commandName, CommandType commandType = CommandType.User, 
            params string[] alias)
        {
            this.commandName = commandName;
            this.commandType = commandType;
            this.commandAlias = alias;
        }
    }

    public enum CommandType
    {
        User,
        Admin
    }
}
