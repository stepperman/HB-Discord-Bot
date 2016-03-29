using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord_Bot.Commands;
using Discord;

namespace Discord_Bot
{
    class AdminCommands
    {
        /// <summary>
        /// Todo: Make this better! This needs to have stuff like 
        /// deleting a user's message, checking for content and if it contains that, delete it,
        /// deleting images, deleting links, it's all cool! 
        /// Sometime later.
        /// </summary>
        public static Func<CommandArgs, Task> DeleteMessages = async e =>
        {
            if (!(Tools.GetPerms(e, e.User) > 1))
                return;

            if (e.Channel.IsPrivate)
                return;

            int deleteNumber = 0;

            Int32.TryParse(e.Args[0], out deleteNumber);

            var messages = await e.Channel.DownloadMessages(deleteNumber + 1);

            foreach (var message in messages)
            {
                await message.Delete();
            }
            await Tools.Reply(e, $"just deleted {deleteNumber} messages on this channel!");
        };

        public static Func<CommandArgs, Task> AddPermissionToRank = async e =>
        {
            var userpermission = Tools.GetPerms(e, e.User);

            if (userpermission >= 1000 || ulong.Parse((string)Program.ProgramInfo.DevID) == e.User.Id)
            {
                var serv = Tools.GetServerInfo(e.Server.Id);

                string RoleToFind = "";
                for (int i = 0; i < e.Args.Length - 1; i++)
                {
                    RoleToFind += e.Args[i] + " ";
                }
                RoleToFind = RoleToFind.Remove(RoleToFind.Length - 1);
                string roleId;

                try
                {
                    roleId = e.Server.FindRoles(RoleToFind).FirstOrDefault().Id.ToString();
                    int permId;
                    int.TryParse(e.Args[e.Args.Length - 1], out permId);

                    if (serv.roleImportancy.ContainsKey(roleId))
                    {
                        serv.roleImportancy[roleId] = permId;
                    }
                    else
                    {
                        serv.roleImportancy.Add(roleId, permId);
                    }

                    Tools.SaveServerInfo();

                    await Tools.Reply(e, $"Successfully added {permId} to {e.Server.GetRole(ulong.Parse(roleId))}.. At least, I think so!");
                }
                catch (Exception ex)
                {
                    await Tools.Reply(e, ex.Message);
                }
            }
        };

        public static Func<CommandArgs, Task> RemovePermissionToRank = async e =>
        {
            var userpermission = Tools.GetPerms(e, e.User);

            if (userpermission >= 1000 || ulong.Parse((string)Program.ProgramInfo.DevID) == e.User.Id)
            {
                try
                {
                    //Parse role
                    string roleName = "";
                    for (int i = 0; i < e.Args.Length; i++)
                    {
                        roleName += e.Args[i] + " ";
                    }
                    //remove the space
                    roleName = roleName.Remove(roleName.Length - 1);

                    //Find the role
                    string roleId;
                    roleId = e.Server.FindRoles(roleName).FirstOrDefault().Id.ToString();


                    var a = Tools.GetServerInfo(e.Server.Id);
                    a.roleImportancy.Remove(roleId);
                    Tools.SaveServerInfo();
                }
                catch (Exception ex)
                {
                    await Tools.Reply(e, ex.Message);
                }
            }
        };

        public static Func<CommandArgs, Task> EditServer = async e =>
        {
            var userpermission = Tools.GetPerms(e, e.User);

            //If not high ranked.
            if (!(userpermission >= 1000))
                return;

            //If there's only 1 word and not anymore, return.
            if (e.Args.Length < 1)
                return;

            ServerInfo info = Tools.GetServerInfo(e.Server.Id);
            string toEdit = e.Args[0];
            string args = "";

            for (int i = 0; i < e.Args.Length; i++)
            {
                if (i == 0)
                    continue;

                args += e.Args[i] + " ";
            }
            args = args.Remove(args.Length - 1);

            switch (toEdit)
            {
                case "standardrole":
                    var role = e.Server.FindRoles(args).FirstOrDefault();
                    info.standardRole = role.Id.ToString();
                    await Tools.Reply(e, $"{role.Name} is now the role that new users will automatically be upon joining the server!");
                    break;
                case "welcomechannel":
                    var channel = e.Server.FindChannels(args, ChannelType.Text).FirstOrDefault();
                    info.welcomingChannel = channel.Id;
                    await Tools.Reply(e, $"{channel.Name} is now the channel that people will be welcomed to upon joining the server!");
                    break;
            }

            Tools.SaveServerInfo();
        };

        public static Func<CommandArgs, Task> KickUser = async e =>
        {
            if (e.Channel.IsPrivate)
                return;

            int userPerm = Tools.GetPerms(e, e.User);
            if (userPerm >= 100)
            {

                User userToKick = Tools.GetUser(e);
                int usertPerms = Tools.GetPerms(e, userToKick);

                if (usertPerms >= userPerm)
                {
                    await Tools.Reply(e, $"You cannot kick {userToKick.Mention}");
                    return;
                }

                if (userToKick == null)
                    return;

                if (userToKick.Id == Program.Client.CurrentUser.Id)
                    return;

                await userToKick.SendMessage($"You've been kicked by {e.User.Name}, you can rejoin by using this url: https://discord.gg/0YOrPxx9u1wtJE0B");
                await Tools.Reply(e, $"just kicked {userToKick.Name}!");
                await userToKick.Kick();
            }
        };

        public static Func<CommandArgs, Task> TimeoutUser = async e =>
        {
            if (e.Args.Count() < 2)
            {
                await Tools.Reply(e, "command was not in the right format. Usage: `/admin timeout {username} {time in minutes}`");
                return;
            }

            if (e.Channel.IsPrivate)
                return;

            int userPerms = Tools.GetPerms(e, e.User);

            if (userPerms > 0)
            {
                User userToTimeOut = Tools.GetUser(e);

                if (userToTimeOut == null || userToTimeOut.Id == Program.Client.CurrentUser.Id)
                {
                    if (userToTimeOut == null)
                        await Tools.Reply(e, "Couldn't find user.");
                    else
                        await Tools.Reply(e, "You cant time me out!");
                    return;
                }

                int timedUserPerms = Tools.GetPerms(e, userToTimeOut);

                if (timedUserPerms >= userPerms)
                {
                    await Tools.Reply(e, $"You cannot timeout {userToTimeOut.Mention} because they're better than you are.");
                    return;
                }

                double minutes = 0;
                try
                {
                    minutes = double.Parse(e.Args[e.Args.Length - 1]);
                }
                catch (FormatException)
                {
                    await Tools.Reply(e, "command was not in the right format. Usage: `/admin timeout {username} {time in minutes}`");
                    return;
                }

                string reply = await Program.timeout.Admin_TimeoutUser(e, minutes, userToTimeOut);
                await Tools.Reply(e, reply);
            }
        };

        public static Func<CommandArgs, Task> GetCommands = async e =>
        {
            string response = $"The character to use a command right now is '{Program._commands.CommandChar}'.\n";
            foreach (var cmd in Program._admincommands._commands)
            {
                if (!String.IsNullOrWhiteSpace(cmd.Purpose))
                {
                    string command = "";
                    foreach (var cmdPart in cmd.Parts)
                        command += cmdPart + ' ';

                    response += $"**{command}** - {cmd.Purpose}";

                    if (cmd.CommandDelay == null)
                        response += "\n";
                    else
                        response += $" **|** Time limit: once per {cmd.CommandDelayNotify} {cmd.timeType}.\n";
                }
            }

            await e.User.SendMessage(response);
        };
    }
}
