using System;
using System.Collections.Generic;
using System.Linq;
using Discord.API;
using Discord.WebSocket;
using Discord;
using System.Reflection;
using qtbot.CommandPlugin.Attributes;

namespace qtbot.CommandPlugin
{
    public partial class CommandsPlugin
    {
        private readonly DiscordSocketClient _client;
        public List<Command> Commands { get; private set; }
        private Func<SocketUser, ulong, int> _getPermissions;

        public char CommandChar { get; set; }
        public bool UseCommandChar { get; set; }

        public CommandsPlugin(DiscordSocketClient client, Func<SocketUser, ulong, int> getPermissions = null, char commandChar = '/', char adminCommandChar = '-')
        {
            _client = client;
            _getPermissions = getPermissions;
            Commands = new List<Command>();

            CommandChar = commandChar;
            UseCommandChar = true;

            BuildCommands();

            var timeValues = new Dictionary<SocketUser, Dictionary<Command, DateTime>>();

            client.MessageReceived += async (message) =>
            {
                //Don't bother to process if there are no commands.
                if (Commands.Count == 0)
                    return;

                //Ignore ourselves.
                if (message.Author.Id == _client.CurrentUser.Id)
                    return;


                string msg = message.Content;
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

                try
                {

                    foreach (var command in Commands)
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

                        //Check if alias
                        if (command.alias != null)
                        {
                            foreach (var alias in command.alias)
                            {
                                if (args[0].Value == alias)
                                {
                                    isValid = true;
                                    break;
                                }
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
                        int permissions = getPermissions != null ? getPermissions(message.Author, ((Discord.ITextChannel)message.Channel).Guild.Id) : 0;
                        var eventArgs = new CommandArgs(message, command, msg, argText, permissions, newArgs);
                        if (permissions < command.MinPerms)
                        {
                            await BotTools.Tools.ReplyAsync(message.Author, message.Channel, "You do not have the correct permissions to use this command.", true);
                            return;
                        }

                        //try to delete the message if the parameter is set.
                        try
                        {
                            if (command.IsHidden && (message.Channel as IPrivateChannel) == null)
                                await message.DeleteAsync();
                        }
                        catch (Exception) { }

                        if (message.Channel as IPrivateChannel == null)
                        {
                            //check if admin, if so he can ignore the time constraint and shit.
                            bool timeCheck = true;
                            if (!command.DelayUnignorable)
                            {
                                var info = BotTools.Tools.GetServerInfo(((ITextChannel)message.Channel).GuildId);
                                if (info.roleImportancy.Count > 0)
                                {
                                    for (int i = 0; i < info.roleImportancy.Count; i++)
                                    {
                                        ulong importantRole = info.roleImportancy.Keys.ToArray()[i];
                                        int importantRoleAmnt = info.roleImportancy.Values.ToArray()[i];
                                        SocketRole role = (message.Channel as ITextChannel)?.Guild.GetRole(importantRole) as SocketRole;

                                        if (role == null) continue;

                                        if ((message.Author as IGuildUser).RoleIds.Contains(role.Id) && importantRoleAmnt >= 15)
                                        {
                                            timeCheck = false;
                                            break;
                                        }
                                    }
                                }
                            }


                            //Check if outside of time limit
                            if (command.CommandDelay != null)
                            {
                                if (timeCheck)
                                {
                                    Dictionary<Command, DateTime> dict;
                                    DateTime time;

                                    //if the user does not have a key, make one. Then get the key.
                                    if (!timeValues.ContainsKey(message.Author))
                                        timeValues.Add(message.Author, new Dictionary<Command, DateTime>());

                                    dict = timeValues[message.Author];

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

                                            if (command.FailHandler == null)
                                                await ((ITextChannel)eventArgs.Channel).SendMessageAsync($"{message.Author.Mention}: You need to wait {waitTime} before you can use /{command.Parts[0]}.");
                                            else
                                            {
                                                try
                                                {
                                                    //why is this here
                                                }
                                                catch (Exception ex)
                                                {
                                                    RaiseCommandError(eventArgs, ex);
                                                }
                                            }
                                            return;
                                        }
                                    }

                                    dict[command] = DateTime.UtcNow;
                                }
                            }
                        }

                        //Run Command
                        Console.WriteLine($"[CommandEvent] {message.Author.Username} used command: {String.Join("", eventArgs.Command.Parts)}.");
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
                            Console.WriteLine(ex);  
                        }
                        break;
                    }

                }
                catch (Exception ex)
                {
                    await BotTools.Tools.ReplyAsync(message.Author, message.Channel, $"Plugin error: {ex.Message}", true);
                    BotTools.Tools.LogError("Error with plugin or something.", ex.Message);
                }

            };

        }
        public CommandBuilder CreateCommand(string cmd)
        {
            var command = new Command(cmd);
            Commands.Add(command);
            return new CommandBuilder(command);
        }

        public void BuildCommands()
        {
            Commands = new List<Command>();
            var q = from t in typeof(Bot).GetTypeInfo().Assembly.GetTypes()
                    where t.GetTypeInfo().IsClass
                    select t;

            q.ToList().ForEach(x =>
            {
                foreach(var method in x.GetMethods())
                {
                    var attribute = method.GetCustomAttribute(typeof(CommandAttribute)) as CommandAttribute;
                    if(attribute != null)
                    {
                        CommandBuilder command = new CommandBuilder(new Command(attribute.commandName));

                        command.Do(method);

                        //Time Delay
                        var cooldownA = method.GetCustomAttribute(typeof(CooldownAttribute)) as CooldownAttribute;
                        if(cooldownA != null)
                        {
                            if (cooldownA.Cooldown == Cooldowns.Seconds)
                                command.SecondDelay(cooldownA.Time);
                            else if (cooldownA.Cooldown == Cooldowns.Minutes)
                                command.MinuteDelay(cooldownA.Time);
                            else if (cooldownA.Cooldown == Cooldowns.Hours)
                                command.HourDelay(cooldownA.Time);
                        }

                        //Add Description to command if it exists.
                        var DescriptionAttribute = method.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
                        if (DescriptionAttribute != null)
                            command.WithPurpose(DescriptionAttribute.Description);

                        var permA = method.GetCustomAttribute(typeof(PermissionAttribute)) as PermissionAttribute;
                        if (permA != null)
                            command.MinPermissions((int)permA.permission);

                        Commands.Add(command._command);
                    }
                }

            });
        }

        internal void AddCommand(Command command)
        {
            Commands.Add(command);
        }
    }
}
