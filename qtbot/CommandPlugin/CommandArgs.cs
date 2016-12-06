using System;
using Discord;
using Discord.WebSocket;

namespace qtbot.CommandPlugin
{
    public class PermissionException : Exception { public PermissionException() : base("User does not have permission to run this command.") { } }
    public class TimeException : Exception { public TimeException() : base("User has too recently used this command.") { } }

    public class CommandArgs
    {
        public SocketMessage Message { get; }
        public Command Command { get; }
        public string CommandText { get; }
        public string ArgText { get; }
        public int? Permssions { get; }
        public string[] Args { get; }

        public SocketUser Author => Message.Author;
        public ulong AuthorId => Message.Author.Id;
        public ITextChannel Channel => (ITextChannel)Message.Channel;
        public string ChannelId => Message.Channel.Id.ToString();
        public SocketGuild Guild => ((ITextChannel)Message.Channel).Guild as SocketGuild;
        public ulong GuildId => Guild.Id;

        public CommandArgs(SocketMessage Message, Command Command, string CommandText, string ArgText, int? Permssions, string[] Args)
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
        public Exception Ex { get; }

        public CommandArgsError(CommandArgs args, Exception ex)
            : base(args.Message, args.Command, args.CommandText, args.ArgText, args.Permssions, args.Args)
        {
            this.Ex = ex;
        }
    }

    public partial class CommandsPlugin
    {
        public event EventHandler<CommandArgs> RanCommand;
        private void RaiseRanCommand(CommandArgs args)
        {
            RanCommand?.Invoke(this, args);
        }

        public event EventHandler<CommandArgs> UnknownCommand;
        private void RaiseUnknownCommand(CommandArgs args)
        {
            UnknownCommand?.Invoke(this, args);
        }

        public event EventHandler<CommandErrorEvent> CommandError;
        private void RaiseCommandError(CommandArgs args, Exception ex)
        {
            CommandError?.Invoke(this, new CommandErrorEvent(args, ex));
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
