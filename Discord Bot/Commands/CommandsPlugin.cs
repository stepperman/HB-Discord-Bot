using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Discord_Bot.Commands
{
    public partial class CommandsPlugin
    {
        private readonly DiscordClient _client;
        public List<Command> _commands { get; private set; }
        private Func<User, int> _getPermissions;

        public char CommandChar { get; set; }
        public bool UseCommandChar { get; set; }

        public CommandsPlugin(DiscordClient client, Func<User, int> getPermissions = null)
        {
            _client = client;
            _getPermissions = getPermissions;
            _commands = new List<Command>();

            CommandChar = '/';
            UseCommandChar = true;

            var timeValues = new Dictionary<User, Dictionary<Command, DateTime>>();

            client.MessageReceived += async (s, e) =>
            {
                //Don't bother to process if there are no commands.
                if (_commands.Count == 0)
                    return;

                //Ignore ourselves.
                if (e.Message.User.Id == _client.CurrentUser.Id)
                    return;


                string msg = e.Message.Text;
                if (UseCommandChar)
                {
                    if (msg.Length == 0)
                        return;

                    if (msg[0] == CommandChar)
                        msg = msg.Substring(1);
                    else
                        return;
                }

                CommandPart[] args;
                if (!CommandParser.ParseArgs(msg, out args))
                    return;

                foreach (var command in _commands)
                {
                    if (args.Length < command.Parts.Length)
                        continue;

                    bool isValid = true;
                    for (int i = 0; i < command.Parts.Length; i++)
                    {
                        if (!string.Equals(args[i].Value, command.Parts[i], StringComparison.OrdinalIgnoreCase))
                        {
                            isValid = false;
                            break;
                        }
                    }
                    if (!isValid)
                        continue;

                    //Check Arg Count.
                    int argCount = args.Length - command.Parts.Length;
                    if (argCount < command.MinArgs || argCount > command.MaxArgs)
                        continue;

                    //Clean Args brb getting drinks got milk kek
                    string[] newArgs = new string[argCount];
                    for (int i = 0; i < newArgs.Length; i++)
                        newArgs[i] = args[i + command.Parts.Length].Value;

                    //Get ArgText
                    string argText;
                    if (argCount == 0)
                        argText = String.Empty;
                    else
                        argText = msg.Substring(args[command.Parts.Length].Index);

                    //Check perms
                    int permissions = getPermissions != null ? getPermissions(e.Message.User) : 0;
                    var eventArgs = new CommandArgs(e.Message, command, msg, argText, permissions, newArgs);
                    if (permissions < command.MinPerms)
                    {
                        RaiseCommandError(eventArgs, new PermissionException());
                        return;
                    }


                    if (command.IsHidden && !e.Channel.IsPrivate)
                        await _client.DeleteMessage(e.Message);

                    //Check if outside of time limit
                    if (command.CommandDelay != null)
                    {
                        Dictionary<Command, DateTime> dict;
                        DateTime time;

                        //if the user does not have a key, make one. Then get the key.
                        if (!timeValues.ContainsKey(e.User))
                            timeValues.Add(e.User, new Dictionary<Command, DateTime>());

                        dict = timeValues[e.User];

                        bool skipTimeCheck = false;
                        //The above gets the time, and if that returns null it adds the current command with the current time to the dict. Then exit the while function.
                        if (!dict.ContainsKey(command))
                        {
                            dict.Add(command, DateTime.UtcNow);
                            skipTimeCheck = true;
                        }

                        if (!skipTimeCheck)
                        {
                            time = dict[command];
                            double test = (DateTime.UtcNow - time).TotalSeconds;
                            if ((DateTime.UtcNow - time).TotalSeconds < command.CommandDelay)
                            {
                                string waitTime = String.Empty;
                                int seconds = (int)(command.CommandDelay - (DateTime.UtcNow - time).TotalSeconds);

                                #region time calculator
                                int days, hours, minutes = 0;

                                minutes = seconds / 60;
                                seconds %= 60;
                                hours = minutes / 60;
                                minutes %= 60;
                                days = hours / 24;
                                hours %= 24;

                                if (days > 0)
                                {
                                    string postfix;
                                    if (days == 1)
                                        postfix = "day";
                                    else
                                        postfix = "days";

                                    waitTime += $"{days} {postfix}";
                                }

                                if (hours > 0)
                                {
                                    if (waitTime.Length > 0)
                                        waitTime += ", ";

                                    string postfix;
                                    if (hours == 1)
                                        postfix = "hour";
                                    else
                                        postfix = "hours";
                                    waitTime += $"{hours} {postfix}";
                                }

                                if (minutes > 0)
                                {
                                    if (waitTime.Length > 0)
                                        waitTime += ", ";

                                    string postfix;
                                    if (minutes == 1)
                                        postfix = "minute";
                                    else
                                        postfix = "minutes";
                                    waitTime += $"{minutes} {postfix}";
                                }

                                if (seconds > 0)
                                {
                                    if (waitTime.Length > 0)
                                        waitTime += " and ";

                                    string postfix;
                                    if (seconds == 1)
                                        postfix = "second";
                                    else
                                        postfix = "seconds";
                                    waitTime += $"{seconds} {postfix}";
                                }
                                #endregion

                                await client.SendMessage(eventArgs.Channel, $"{Mention.User(e.User)}: You need to wait {waitTime} before you can use /{command.Parts[0]}.");
                                RaiseCommandError(eventArgs, new TimeException());
                                return;
                            }
                        }

                        dict[command] = DateTime.UtcNow;
                    }


                    //Run Command
                    Console.WriteLine($"[CommandEvent] {e.User.Name} used command: {String.Join("", eventArgs.Command.Parts)}.");
                    RaiseRanCommand(eventArgs);
                    try
                    {
                        var task = command.Handler(eventArgs);
                        if (task != null)
                            await task;
                    }
                    catch (Exception ex)
                    {
                        RaiseCommandError(eventArgs, ex);
                    }
                    break;

                }

            };


        }
        public void CreateCommandGroup(string cmd, Action<CommandGroupBuilder> config = null)
            => config(new CommandGroupBuilder(this, cmd, 0));
        public CommandBuilder CreateCommand(string cmd)
        {
            var command = new Command(cmd);
            _commands.Add(command);
            return new CommandBuilder(command);
        }

        internal void AddCommand(Command command)
        {
            _commands.Add(command);
        }
    }
}
