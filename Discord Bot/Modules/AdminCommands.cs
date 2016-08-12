using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord_Bot.CommandPlugin;
using System.Text.RegularExpressions;
using Discord;

namespace Discord_Bot
{
    class AdminCommands
    {
        public static Func<CommandArgs, Task> DeleteMessages = async e =>
        {
            if (e.Channel.IsPrivate)
                return;

            bool silent = e.Args[e.Args.Count() - 1] == "-s";

            if(silent)
                await e.Message.Delete();

            //delete num
            int deleteNumber = 0;
            if (Int32.TryParse(e.Args[0], out deleteNumber))
            {

                var messages = await e.Channel.DownloadMessages(deleteNumber + 1);

                foreach (var message in messages)
                {
                    await message.Delete();
                }

                if (!silent)
                    await Tools.Reply(e, $"deleted {deleteNumber} messages!");
            }
            //Delete users' messages.
            else if (e.Message.MentionedUsers.Count() != 0)
            {
                var messages = await e.Channel.DownloadMessages();

                var potentials = new List<Message>();

                foreach (var msg in messages)
                {
                    if (e.Message.MentionedUsers.Contains(msg.User))
                        potentials.Add(msg);
                }

                if (potentials.Count() == 0)
                    return;

                foreach (var msg in potentials)
                {
                    await msg.Delete();
                }

                deleteNumber = potentials.Count();

                string users = "";
                for (int i = 0; i < e.Message.MentionedUsers.Count(); i++)
                {
                    if (i != 0)
                        users += ", ";
                    users += e.Message.MentionedUsers.ToArray()[i].Name; 
                }

                if (!silent)
                    await Tools.Reply(e, $"deleted {deleteNumber} messages by {users}!");
            }
            //embedded stuff
            else if (e.ArgText.StartsWith("embed"))
            {
                var messages = await e.Channel.DownloadMessages();

                var potentials = new List<Message>();

                foreach (var msg in messages)
                {
                    if (msg.Embeds.Count() != 0)
                        potentials.Add(msg);
                }

                foreach (var msg in potentials)
                {
                    await msg.Delete();
                }

                deleteNumber = potentials.Count();

                if (!silent)
                    await Tools.Reply(e, $"deleted {deleteNumber} messages with embedded content!");
            }
            else if (e.ArgText.StartsWith("img"))
            {
                var messages = await e.Channel.DownloadMessages();

                var potentials = new List<Message>();

                foreach (var msg in messages)
                {
                    if (msg.Attachments.Count() != 0)
                        potentials.Add(msg);
                }

                foreach (var msg in potentials)
                {
                    await msg.Delete();
                }

                deleteNumber = potentials.Count();

                if (!silent)
                    await Tools.Reply(e, $"deleted {deleteNumber} images!");
            }
            else
            {
                try
                {
                    Regex regex = new Regex("\"[^\"]*\"");
                    string text = regex.Match(e.ArgText).Value.TrimStart('"').TrimEnd('"');

                    if (text == "")
                        return;

                    var messages = await e.Channel.DownloadMessages();

                    var potentials = new List<Message>();

                    foreach (var msg in messages)
                    {
                        if (msg.Text.ToLower().Contains(text.ToLower()))
                            potentials.Add(msg);
                    }

                    foreach (var msg in potentials)
                    {
                        await msg.Delete();
                    }


                    deleteNumber = potentials.Count();

                    if (!silent)
                        await Tools.Reply(e, $"deleted {deleteNumber} messages containing `{text}`!");
                }
                catch (Exception) { }
            }

            
                
        };

        public static Func<CommandArgs, Task> AddPermissionToRank = async e =>
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
                    roleId = e.Server.FindRoles(RoleToFind, false).FirstOrDefault().Id.ToString();
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
        };

        public static Func<CommandArgs, Task> RemovePermissionToRank = async e =>
        {
            var userpermission = Tools.GetPerms(e, e.User);
            
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
        };

        public static Func<CommandArgs, Task> EditServer = async e =>
        {
            var userpermission = Tools.GetPerms(e, e.User);
            
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
            
            if(args != "")
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
                case "safesearch":

                    if (args == "")
                    {
                        await Tools.Reply(e, "Possible options are `high, medium, off`. Otherwise it will fuck.");
                        return;
                    }

                    info.safesearch = args;
                    break;
                case "regular":
                    await Tools.Reply(e, "`regularrole` for roles.\n`regulerenabled` (false, true) to enable/disable.\n`regularamount` for amount." + 
                        "\n'regulartime' to set the time.");
                    break;
                case "regularrole":
                    var regrole = e.Server.FindRoles(args).FirstOrDefault();
                    info.RegularUserRoleId = regrole.Id;
                    await Tools.Reply(e, $"{regrole.Name} is now the role that regular users will receive");
                    break;
                case "regularenabled":

                    if (args == "true")
                    {
                        info.RegularUsersEnabled = true;
                        await Tools.Reply(e, "Regular User Role is now enabled. Bastard.");
                    }
                    else
                    {
                        info.RegularUsersEnabled = false;
                        await Tools.Reply(e, "Regular User Role is now disabled. Good.");
                    }

                    break;
                case "regularamount":

                    var amount = int.Parse(args);
                    info.RegularUserMinMessages = amount;

                    break;
                case "regulartime":
                    var time = double.Parse(args);
                    info.RegularUserMinutesPerMessage = time;
                    break;
            }

            Tools.SaveServerInfo();
        };

        public static Func<CommandArgs, Task> KickUser = async e =>
        {
            if (e.Channel.IsPrivate)
                return;

            int userPerm = Tools.GetPerms(e, e.User);

                User userToKick = Tools.GetUser(e);
                int usertPerms = Tools.GetPerms(e, userToKick);

                if (usertPerms >= userPerm)
                {
                    await Tools.Reply(e, $"You cannot kick {userToKick.Mention}");
                    return;
                }

                if (userToKick == null)
                    return;

                if (userToKick.Id == Storage.client.CurrentUser.Id)
                    return;

                await userToKick.SendMessage($"You've been kicked by {e.User.Name}, you can rejoin by using this url: https://discord.gg/0YOrPxx9u1wtJE0B");
                await Tools.Reply(e, $"just kicked {userToKick.Name}!");
                await userToKick.Kick();
        };

        public static Func<CommandArgs, Task> TimeoutUser = async e =>
        {
            if (e.Args.Count() < 2)
            {
                await Tools.Reply(e, "command was not in the right format. Usage: `-timeout {username(s)} {time in minutes}`");
                return;
            }

            if (e.Channel.IsPrivate)
                return;

            int userPerms = Tools.GetPerms(e, e.User);

            if (userPerms > 0)
            {
                var mentionedUsers = e.Message.MentionedUsers.ToArray();
                
                if (mentionedUsers[0] == null || mentionedUsers[0].Id == Storage.client.CurrentUser.Id)
                {
                    if (mentionedUsers[0] == null)
                        await Tools.Reply(e, "Couldn't find user.");
                    else
                        await Tools.Reply(e, "You cant time me out!");
                    return;
                }

                string message = "";

                for (int i = 0; i < mentionedUsers.Length; i++)
                {
                    int timedUserPerms = Tools.GetPerms(e, mentionedUsers[i]);

                    if (timedUserPerms >= userPerms)
                    {
                        message += $"You cannot timeout {mentionedUsers[i].Mention} because they're better than you are.\n";
                        continue;
                    }

                    double minutes = 0;
                    try
                    {
                        minutes = double.Parse(e.Args[e.Args.Length - 1]);
                    }
                    catch (FormatException)
                    {
                        await Tools.Reply(e, "command was not in the right format. Usage: `-timeout {username(s)} {time in minutes}`");
                        return;
                    }

                    message += await Program.timeout.Admin_TimeoutUser(e, minutes, mentionedUsers[i]) + "\n";
                }

                await Tools.Reply(e, message);
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


        //Role management
        public static Func<CommandArgs, Task> AddRemoveRole = async e =>
        {
            var Args = e.Message.RawText.Split(' ');
            Args = Args.Skip(1).ToArray();

            if (Args.Length < 3 || e.Message.MentionedUsers.Count() == 0 || e.Message.MentionedUsers.Any(x => x.Id == e.User.Id))
            {
                await RoleSuccessFail(false, e);
                return;
            }

            string roleName = "";
            for (int i = 1; i < Args.Length; i++) //i starts with 1 to avoid the "remove" or "add" arg
            {
                //If this parameter starts with a user mention, skip this.
                if (Args[i].StartsWith("<") && Args[i].EndsWith(">"))
                    continue;

                //If it's not a user mention, add it to the roleName.
                roleName += Args[i] + " ";
            }

            roleName = roleName.Trim();

            //Find the role
            Role roleToGive;
            roleToGive = e.Server.FindRoles(roleName).FirstOrDefault();

            if (roleToGive == null)
            {
                await RoleSuccessFail(false, e);
                return;
            }
                

            //Give or add role
            switch (Args[0].ToLower())
            {
                case "add":
                case "a":

                    foreach (var user in e.Message.MentionedUsers)
                    {
                        var userRoles = user.Roles.ToList();
                        if (userRoles.Any(x => x.Id == roleToGive.Id))
                            continue;

                        userRoles.Add(roleToGive);
                        try { await user.Edit(null, null, null, userRoles); }
                        catch (Exception)
                        {
                            Console.WriteLine($"Couldn't edit {user.Name}");
                        }
                    }

                    await RoleSuccessFail(true, e);

                    break;
                case "remove":
                case "r":

                    foreach (var user in e.Message.MentionedUsers)
                    {
                        var userRoles = user.Roles.ToList();
                        if (!userRoles.Any(x => x.Id == roleToGive.Id))
                            continue;

                        userRoles.RemoveAll(x => x.Id == roleToGive.Id);
                        try { await user.Edit(null, null, null, userRoles); } catch (Exception)
                        {
                            Console.WriteLine($"Couldn't edit {user.Name}");
                        }
                    }

                    await RoleSuccessFail(true, e);

                    break;
                default:
                    return;
            }
        };

        public static async Task RoleSuccessFail(bool success, CommandArgs e)
        {
            if (success)
                await Tools.Reply(e, "👌🏽", false);
            else
                await Tools.Reply(e, "🖕🏽", false);
        }
    }
}
