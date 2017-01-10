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
        public static Modules.Timeout timeout;

        public async Task StartAsync()
        {
            var client = new DiscordSocketClient();
            Bot.Client = client;
            Bot.Client.Log += Client_Log;

            _commands = new CommandsPlugin(Bot.Client);
            _admincommands = new CommandsPlugin(Bot.Client, (e, s) => { return BotTools.Tools.GetPerms(s, e as IGuildUser); }, '-');
            _commands.CreateCommandGroup("", group => BuildCommands(group));
            _admincommands.CreateCommandGroup("", adminGroup => BuildAdminCommands(adminGroup));

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
                timeout = new Modules.Timeout(Client);
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
        private static void BuildCommands(CommandGroupBuilder group)
        {
            group.DefaultMinPermissions(0);

            group.CreateCommand("al")
                .WithPurpose("Get anilist user.")
                .ArgsAtLeast(1)
                .Do(Modules.AnimeTools.UserFromAnilist);

            group.CreateCommand("source")
                .WithPurpose("Link to my Github baby ;))))")
                .Do(async e =>
                    await BotTools.Tools.ReplyAsync(e, "Here's my source code! <https://github.com/stepperman/HB-Discord-Bot>"));

            group.CreateCommand("yt").Alias("youtube")
                .ArgsAtLeast(1)
                .WithPurpose("Find YouTube videos!")
                .Do(qtbot.Modules.YouTube.FindYouTubeVideo);

            group.CreateCommand("anime")
                .ArgsAtLeast(1)
                .WithPurpose("Get an anime from Anilist.")
                .Do(qtbot.Modules.AnimeTools.AnimeFromAnilist);

            group.CreateCommand("uptime")
                .Do(qtbot.Modules.Uptime.ShowUptime)
                .WithPurpose("show the bot's uptime.");

            /*
            group.CreateCommand("bullying")
                .Do(qtbot.Modules.Fun.Bullying)
                .WithPurpose("Getting bullied? Make sure to use this command as often as possible!")
                .MinuteDelay(1);
                Goodbye again, bullying.
    */
            group.CreateCommand("downtime")
                .Do(async e =>
                    await BotTools.Tools.ReplyAsync(e, "a **long time.**"));

            group.CreateCommand("ud")
                .WithPurpose("Find the definition of a word with Urban Dictionary.")
                .Do(Modules.Fun.UrbanDictionary);

            group.CreateCommand("hb")
                .WithPurpose("Find a User's HummingBird account with it's information!")
                .ArgsAtMax(1)
                .IsHidden()
                .Do(Modules.AnimeTools.GetHBUser);

            group.CreateCommand("8ball")
                .WithPurpose("The magic eightball will answer all your doubts and questions!")
                .AnyArgs()
                .SecondDelay(20)
                .Do(Modules.Fun.EightBall);

            group.CreateCommand("lmao")
                .AnyArgs()
                .HourDelay(1)
                .Do(async e =>
                    await BotTools.Tools.ReplyAsync(e, "https://www.youtube.com/watch?v=HTLZjhHIEdw"));

            group.CreateCommand("no")
                .SecondDelay(120)
                .AnyArgs()
                .Do(async e =>
                    await BotTools.Tools.ReplyAsync(e, "pignig", false));

            group.CreateCommand("ayy")
                .MinuteDelay(2)
                .AnyArgs()
                .Do(Modules.Fun.Ayy);

            group.CreateCommand("commands")
                .AnyArgs()
                .IsHidden()
                .Do(Modules.Information.Commands);

            group.CreateCommand("img")
                .MinuteDelay(1)
                .ArgsAtLeast(1)
                .WithPurpose("Get an image of Google. (100 per day pls)")
                .Do(Modules.Fun.GetImageFromGoogleDotCom);
        }

        private static void BuildAdminCommands(CommandGroupBuilder adminGroup)
        {
            adminGroup.DefaultMinPermissions(90);

            adminGroup.CreateCommand("change nickname").Alias("nick")
                .MinPermissions(1000)
                .WithPurpose("Change the bot's username.")
                .ArgsAtLeast(1)
                .Do(AdminCommands.ChangeNickname);

            adminGroup.CreateCommand("change username").Alias("username")
                .MinPermissions(8000)
                .ArgsAtLeast(1)
                .Do(AdminCommands.ChangeUsername);

            adminGroup.CreateCommand("change avatar").Alias("avatar")
                .MinPermissions(1000)
                .WithPurpose("Change the bot's avatar.")
                .ArgsAtLeast(1)
                .Do(AdminCommands.ChangeAvatar);

            adminGroup.CreateCommand("role").Alias("r")
                .MinPermissions(500)
                .WithPurpose("Remove or add a role. Usage: `-role add/remove @{user(s)} Role name`")
                .Do(AdminCommands.AddRemoveRole);

            adminGroup.CreateCommand("delete").Alias("d", "remove")
                .WithPurpose("Delete messages on this channel. Usage: `/admin delete {number of messages to delete}`. / req: rank perm > 0")
                .ArgsAtLeast(1)
                .Do(AdminCommands.DeleteMessages);

            adminGroup.CreateCommand("addpermission")
                .WithPurpose("Add number to rank. Usage: `/admin addpermission {rank name} {number}` / req: rank perm >= 1000")
                .Do(AdminCommands.AddPermissionToRank);

            adminGroup.CreateCommand("removePerm")
                .WithPurpose("Remove number of rank. Usage: `/admin addpermission {rank name}` / req: rank perm >= 1000")
                .Do(AdminCommands.RemovePermissionToRank);

            adminGroup.CreateCommand("editServer")
                .WithPurpose("standardrole or welcomechannel. / req: rank perm >= 1000")
                .Do(AdminCommands.EditServer);

            adminGroup.CreateCommand("timeout").Alias("t")
                .WithPurpose("Time out someone. Usage: `/admin timeout {@username} {time in minutes}`.")
                .ArgsAtLeast(1)
                .Do(AdminCommands.TimeoutUser);

            adminGroup.CreateCommand("commands")
                .IsHidden()
                .AnyArgs()
                .Do(AdminCommands.GetCommands);

            adminGroup.CreateCommand("save")
                .Do(async e =>
                {
                    if (BotTools.Storage.programInfo.DevID == e.Author.Id)
                        await Modules.RegularUsers.SaveAsync();
                });

            adminGroup.CreateCommand("clearperms")
                .MinPermissions(999)
                .Do(async e =>
                {
                    var serverInfo = BotTools.Tools.GetServerInfo(e.Guild.Id);
                    serverInfo.roleImportancy.Clear();
                    await Task.Delay(0);
                    BotTools.Tools.SaveServerInfo();
                });
        }
        #endregion
    }
}
