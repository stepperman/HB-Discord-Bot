using System;
using System.Reflection;
using System.Threading.Tasks;

namespace qtbot.CommandPlugin
{
    public sealed class CommandBuilder
    {
        public Command cmd;
        public CommandBuilder(Command command)
        {
            cmd = command;
        }

        public CommandBuilder ArgsEqual(int argCount)
        {
            cmd.MinArgs = argCount;
            cmd.MaxArgs = argCount;
            return this;
        }

        public CommandBuilder SecondDelay(int seconds)
        {
            if (seconds == 1)
                cmd.timeType = "second";
            else
                cmd.timeType = "seconds";
            cmd.CommandDelay = seconds;
            cmd.CommandDelayNotify = seconds;
            return this;
        }

        public CommandBuilder MinuteDelay(int minutes)
        {
            if (minutes == 1)
                cmd.timeType = "minute";
            else
                cmd.timeType = "minutes";
            cmd.CommandDelay = minutes * 60;
            cmd.CommandDelayNotify = minutes;
            return this;
        }

        public CommandBuilder HourDelay(int hours)
        {
            if (hours == 1)
                cmd.timeType = "hour";
            else
                cmd.timeType = "hours";
            cmd.CommandDelay = hours * 60 * 60;
            cmd.CommandDelayNotify = hours;
            return this;
        }

        public CommandBuilder DayDelay(int days)
        {
            if (days == 1)
                cmd.timeType = "day";
            else
                cmd.timeType = "days";
            cmd.CommandDelay = days * 60 * 60 * 24;
            cmd.CommandDelayNotify = days;
            return this;
        }

        public CommandBuilder WithPurpose(string purpose)
        {
            cmd.Purpose = purpose;
            return this;
        }

        public CommandBuilder ArgsAtLeast(int minArgCount)
        {
            cmd.MinArgs = minArgCount;
            cmd.MaxArgs = null;
            return this;
        }

        public CommandBuilder ArgsAtMax(int maxArgCount)
        {
            cmd.MaxArgs = maxArgCount;
            cmd.MinArgs = null;
            return this;
        }

        public CommandBuilder ArgsBetween(int minArg, int maxArg)
        {
            cmd.MaxArgs = maxArg;
            cmd.MinArgs = minArg;
            return this;
        }

        public CommandBuilder AnyArgs()
        {
            cmd.MaxArgs = null;
            cmd.MinArgs = null;
            return this;
        }

        public CommandBuilder Alias(params string[] alias)
        {
            cmd.SetAliases(alias);
            return this;
        }

        public CommandBuilder NoArgs()
        {
            cmd.MinArgs = 0;
            cmd.MaxArgs = 0;
            return this;
        }

        public CommandBuilder MinPermissions(int level)
        {
            cmd.MinPerms = level;
            return this;
        }

        public CommandBuilder IsHidden()
        {
            cmd.IsHidden = true;
            return this;
        }

        public CommandBuilder DelayIsUnignorable()
        {
            cmd.DelayUnignorable = true;
            return this;
        }

        public CommandBuilder Do(MethodInfo func)
        {
            cmd.Handler = (e) =>
            {
                return func.Invoke(null, new object[] { e }) as Task ?? Task.Delay(0);
            };
            return this;
        }

        public CommandBuilder OnFail(Func<CommandArgs, Task> func)
        {
            cmd.FailHandler = func;
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
