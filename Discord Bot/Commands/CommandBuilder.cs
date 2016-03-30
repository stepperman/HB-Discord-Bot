using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public sealed class CommandBuilder
    {
        private readonly Command _command;
        public CommandBuilder(Command command)
        {
            _command = command;
        }

        public CommandBuilder ArgsEqual(int argCount)
        {
            _command.MinArgs = argCount;
            _command.MaxArgs = argCount;
            return this;
        }

        public CommandBuilder SecondDelay(int seconds)
        {
            if (seconds == 1)
                _command.timeType = "second";
            else
                _command.timeType = "seconds";
            _command.CommandDelay = seconds;
            _command.CommandDelayNotify = seconds;
            return this;
        }

        public CommandBuilder MinuteDelay(int minutes)
        {
            if (minutes == 1)
                _command.timeType = "minute";
            else
                _command.timeType = "minutes";
            _command.CommandDelay = minutes * 60;
            _command.CommandDelayNotify = minutes;
            return this;
        }

        public CommandBuilder HourDelay(int hours)
        {
            if (hours == 1)
                _command.timeType = "hour";
            else
                _command.timeType = "hours";
            _command.CommandDelay = hours * 60 * 60;
            _command.CommandDelayNotify = hours;
            return this;
        }

        public CommandBuilder DayDelay(int days)
        {
            if (days == 1)
                _command.timeType = "day";
            else
                _command.timeType = "days";
            _command.CommandDelay = days * 60 * 60 * 24;
            _command.CommandDelayNotify = days;
            return this;
        }

        public CommandBuilder WithPurpose(string purpose)
        {
            _command.Purpose = purpose;
            return this;
        }

        public CommandBuilder ArgsAtLeast(int minArgCount)
        {
            _command.MinArgs = minArgCount;
            _command.MaxArgs = null;
            return this;
        }

        public CommandBuilder ArgsAtMax(int maxArgCount)
        {
            _command.MaxArgs = maxArgCount;
            _command.MinArgs = null;
            return this;
        }

        public CommandBuilder ArgsBetween(int minArg, int maxArg)
        {
            _command.MaxArgs = maxArg;
            _command.MinArgs = minArg;
            return this;
        }

        public CommandBuilder AnyArgs()
        {
            _command.MaxArgs = null;
            _command.MinArgs = null;
            return this;
        }

        public CommandBuilder NoArgs()
        {
            _command.MinArgs = 0;
            _command.MaxArgs = 0;
            return this;
        }

        public CommandBuilder MinPermissions(int level)
        {
            _command.MinPerms = level;
            return this;
        }

        public CommandBuilder IsHidden()
        {
            _command.IsHidden = true;
            return this;
        }

        public CommandBuilder DelayIsUnignorable()
        {
            _command.DelayUnignorable = true;
            return this;
        }

        public CommandBuilder Do(Func<CommandArgs, Task> func)
        {
            _command.Handler = func;
            return this;
        }

        public CommandBuilder OnFail(Func<CommandArgs, Task> func)
        {
            _command.FailHandler = func;
            return this;
        }
    }

    public sealed class CommandGroupBuilder
    {
        private readonly CommandsPlugin _plugin;
        private readonly string _prefix;
        private int _defaultMinPermissions;

        internal CommandGroupBuilder(CommandsPlugin plugin, string prefix, int minPerm)
        {
            _plugin = plugin;
            _prefix = prefix;
            _defaultMinPermissions = minPerm;
        }

        public void DefaultMinPermissions(int min)
        {
            _defaultMinPermissions = min;
        }

        public CommandGroupBuilder CreateCommandGroup(string cmd, Action<CommandGroupBuilder> config = null)
        {
            config(new CommandGroupBuilder(_plugin, _prefix + ' ' + cmd, _defaultMinPermissions));
            return this;
        }

        public CommandBuilder CreateCommand(string cmd)
        {
            string text;
            if(cmd != "")
            {
                if (_prefix != "")
                    text = _prefix + ' ' + cmd;
                else
                    text = cmd;
            }
            else
            {
                if (_prefix != "")
                    text = _prefix;
                else
                    throw new ArgumentOutOfRangeException(nameof(cmd));
            }

            var command = new Command(text);
            command.MinPerms = _defaultMinPermissions;
            _plugin.AddCommand(command);
            return new CommandBuilder(command);
        }
    }
}
