using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using qtbot.CommandPlugin;

namespace qtbot
{
    class Bot
    {
        public static DiscordSocketClient Client { get; private set; }
        public static CommandsPlugin _commands, _admincommands;
        public static Modules.OldTimeout timeout;

        public async Task StartAsync()
        {
            var client = new DiscordSocketClient();
            Bot.Client = client;
            Bot.Client.Log += Client_Log;

            _commands = new CommandsPlugin(Bot.Client);
            _admincommands = new CommandsPlugin(Bot.Client, (e, s) => { return BotTools.Tools.GetPerms(s, e as IGuildUser); }, '-');

            BotTools.Setup.GetProgramInfo();

            Bot.Client.UserJoined += BotTools.ChatEventActions.UserJoinedAsync;
            Bot.Client.UserLeft += BotTools.ChatEventActions.UserLeftAsync;
            Bot.Client.UserBanned += BotTools.ChatEventActions.UserBannedAsync;
            Bot.Client.MessageReceived += BotTools.ChatEventActions.MessageReceivedAsync;
            Bot.Client.Disconnected += BotTools.ChatEventActions.DisconnectedAsync;


            _commands.CommandError += async (s, e) =>
            {
                if (e.Command.Text == "ayy")
                    return;

                var ex = e.Exception;
                if (ex is PermissionException)
                    await BotTools.Tools.ReplyAsync(e, "Sorry, you do not have the permissions to use this command!");
                else
                    await BotTools.Tools.ReplyAsync(e, $"Error: {ex.Message}.");

            };


            await Modules.AnimeTools.AuthorizeAnilistAsync();
            await LoginAsync();

            await Task.Delay(-1);

        }

        public async Task LoginAsync()
        {
            try
            {
                await Client.LoginAsync(TokenType.Bot, (string)BotTools.Storage.programInfo.bot_token);
                await Client.ConnectAsync();
                timeout = new Modules.OldTimeout(Client);
                BotTools.Storage.client = Client;
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }

        private Task Client_Log(LogMessage arg)
        {
            Console.WriteLine($"[{arg.Severity}] {arg.Source}: {arg.Message}.");
            var f = System.IO.File.CreateText("errors.txt");
            f.WriteLine(arg.Exception?.ToString() + "\n\n");
            f.Dispose();
            return Task.CompletedTask;
        }

        #region commands

        private static void BuildAdminCommands(CommandGroupBuilder adminGroup)
        {
            //adminGroup.CreateCommand("role").Alias("r")
            //    .MinPermissions(500)
            //    .WithPurpose("Remove or add a role. Usage: `-role add/remove @{user(s)} Role name`")
            //    .Do(AdminCommands.AddRemoveRole);

                //adminGroup.CreateCommand("delete").Alias("d", "remove")
                //    .WithPurpose("Delete messages on this channel. Usage: `/admin delete {number of messages to delete}`. / req: rank perm > 0")
                //    .ArgsAtLeast(1)
                //    .Do(AdminCommands.DeleteMessages);

            //    adminGroup.CreateCommand("addpermission")
            //        .WithPurpose("Add number to rank. Usage: `/admin addpermission {rank name} {number}` / req: rank perm >= 1000")
            //        .Do(AdminCommands.AddPermissionToRank);

            //    adminGroup.CreateCommand("removePerm")
            //        .WithPurpose("Remove number of rank. Usage: `/admin addpermission {rank name}` / req: rank perm >= 1000")
            //        .Do(AdminCommands.RemovePermissionToRank);

            //    adminGroup.CreateCommand("editServer")
            //        .WithPurpose("standardrole or welcomechannel. / req: rank perm >= 1000")
            //        .Do(AdminCommands.EditServer);

            //    adminGroup.CreateCommand("timeout").Alias("t")
            //        .WithPurpose("Time out someone. Usage: `/admin timeout {@username} {time in minutes}`.")
            //        .ArgsAtLeast(1)
            //        .Do(AdminCommands.TimeoutUser);

            //    adminGroup.CreateCommand("commands")
            //        .IsHidden()
            //        .AnyArgs()
            //        .Do(AdminCommands.GetCommands);

            //    adminGroup.CreateCommand("save")
            //        .Do(async e =>
            //        {
            //            if (BotTools.Storage.programInfo.DevID == e.Author.Id)
            //                await Modules.RegularUsers.SaveAsync();
            //        });

            //    adminGroup.CreateCommand("clearperms")
            //        .MinPermissions(999)
            //        .Do(async e =>
            //        {
            //            var serverInfo = BotTools.Tools.GetServerInfo(e.Guild.Id);
            //            serverInfo.roleImportancy.Clear();
            //            await Task.Delay(0);
            //            BotTools.Tools.SaveServerInfo();
            //        });
            //}
            #endregion
        }
    }
