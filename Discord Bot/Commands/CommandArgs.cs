using System;
using Discord;

namespace Discord_Bot.Commands
{
    public class PermissionException : Exception { public PermissionException() : base("User does not have permission to run this command.") { } }
    public class TimeException : Exception { public TimeException() : base("User has too recently used this command.") { } }

    public class CommandArgs
    {
        public Message Message { get; }
        public Command Command { get; }
        public string CommandText { get; }
        public string ArgText { get; }
        public int? Permssions { get; }
        public string[] Args { get; }

        public User User => Message.User;
        public string userId => Message.User.Id.ToString();
        public Channel Channel => Message.Channel;
        public string channelId => Message.Channel.Id.ToString();
        public Server Server => Message.Server;
        public string serverId => Message.Server.Id.ToString();

        public CommandArgs(Message Message, Command Command, string CommandText, string ArgText, int? Permssions, string[] Args)
        {
            this.Message = Message;
            this.Command = Command;
            this.CommandText = CommandText;
            this.ArgText = ArgText;
            this.Permssions = Permssions;
            this.Args = Args;
        }
    }

    public class CommandArgsError : CommandArgs
    {
        public Exception ex { get; }

        public CommandArgsError(CommandArgs args, Exception ex)
            : base(args.Message, args.Command, args.CommandText, args.ArgText, args.Permssions, args.Args)
        {
            this.ex = ex;
        }
    }

    public partial class CommandsPlugin
    {
        public event EventHandler<CommandArgs> RanCommand;
        private void RaiseRanCommand(CommandArgs args)
        {
            if (RanCommand != null)
                RanCommand(this, args);
        }

        public event EventHandler<CommandArgs> UnknownCommand;
        private void RaiseUnknownCommand(CommandArgs args)
        {
            if (UnknownCommand != null)
                UnknownCommand(this, args);
        }

        public event EventHandler<CommandErrorEvent> CommandError;
        private void RaiseCommandError(CommandArgs args, Exception ex)
        {
            if (CommandError != null)
                CommandError(this, new CommandErrorEvent(args, ex));
        }
    }

    public class CommandErrorEvent : CommandArgs
    {
        public Exception Exception { get; }

        public CommandErrorEvent(CommandArgs arg, Exception ex)
            : base(arg.Message, arg.Command, arg.CommandText, arg.ArgText, arg.Permssions, arg.Args)
        {
            Exception = ex;
        }
    }
}
