using System;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord_Bot.CommandPlugin;

namespace Discord_Bot
{
    class Bot
    {
        public static DiscordClient client;
        public static Timeout timeout;
        public static CommandsPlugin _commands, _admincommands;

        public Bot()
        {
            //So the bot trusts anything
            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

            var client = new DiscordClient();
            Bot.client = client;
            Bot.client.Log.Message += (s, e) => Console.WriteLine($"[{e.Severity}] {e.Source}: {e.Message}");

            _commands = new CommandsPlugin(Bot.client);
            _admincommands = new CommandsPlugin(Bot.client, (e, s) => { return Tools.GetPerms(s, e); }, '-');
            _commands.CreateCommandGroup("", group => BuildCommands(group));
            _admincommands.CreateCommandGroup("", adminGroup => BuildAdminCommands(adminGroup));

            //Get Programinfo
            Setup.GetProgramInfo();

            Bot.client.UserJoined += ChatEventActions.UserJoined;
            Bot.client.UserLeft += ChatEventActions.UserLeft;
            Bot.client.UserBanned += ChatEventActions.UserBanned;
            Bot.client.MessageReceived += ChatEventActions.MessageReceived;
            Bot.client.GatewaySocket.Disconnected += ChatEventActions.Disconnected;

            _commands.CommandError += async (s, e) =>
            {
                if (e.Command.Text == "ayy")
                    return;

                var ex = e.Exception;
                if (ex is PermissionException)
                    await Tools.Reply(e, "Sorry, you do not have the permissions to use this command!");
                else
                    await Tools.Reply(e, $"Error: {ex.Message}.");

            };

            AnimeTools.AuthorizeAnilist().Wait(); ;
            Login();
        }

        public void Login()
        {
            try
            {
                client.ExecuteAndWait(async () =>
                {
                    var nottoken = Convert.ToString(Storage.programInfo.bot_token);

                    await client.Connect(nottoken);
                    timeout = new Timeout(client);
                    Storage.client = client;
                });
            }
            catch (Discord.Net.HttpException)
            {
            }
        }


        #region commands
        private static void BuildCommands(CommandGroupBuilder group)
        {
            group.DefaultMinPermissions(0);

            group.CreateCommand("source")
                .WithPurpose("Link to my Github baby ;))))")
                .Do(async e =>
                    await Tools.Reply(e, "Here's my source code! <https://github.com/stepperman/HB-Discord-Bot>"));

            group.CreateCommand("yt").Alias("youtube")
                .ArgsAtLeast(1)
                .WithPurpose("Find YouTube videos!")
                .Do(Discord_Bot.Modules.YouTube.FindYouTubeVideo);

            group.CreateCommand("anime")
                .ArgsAtLeast(1)
                .WithPurpose("Get an anime from Anilist.")
                .Do(AnimeTools.AnimeFromAnilist);

            group.CreateCommand("uptime")
                .Do(Uptime.ShowUptime)
                .WithPurpose("show the bot's uptime.");

            group.CreateCommand("bullying")
                .Do(Fun.Bullying)
                .WithPurpose("Getting bullied? Make sure to use this command as often as possible!")
                .MinuteDelay(1);

            group.CreateCommand("downtime")
                .Do(async e =>
                    await Tools.Reply(e, "a **long time.**"));

            group.CreateCommand("ud")
                .WithPurpose("Find the definition of a word with Urban Dictionary.")
                .Do(Fun.UrbanDictionary);

            group.CreateCommand("hidechannel")
                .WithPurpose("Hide a channel. Usage: `/hidechannel #{channel}` This doesn't work if you have the administrator role.")
                .ArgsAtLeast(1)
                .IsHidden()
                .Do(Modules.HideChannel.Hide);

            group.CreateCommand("showchannel")
                .WithPurpose("Show a channel you have hid. Usage: `/showchannel {channelnum}`. Check the channelnums with the listhcannels command.")
                .ArgsEqual(1)
                .IsHidden()
                .Do(Modules.HideChannel.Show);

            group.CreateCommand("listhchannels")
                .WithPurpose("Show all the channels you have hid.")
                .NoArgs()
                .IsHidden()
                .Do(Modules.HideChannel.List);

            group.CreateCommand("hb")
                .WithPurpose("Find a User's HummingBird account with it's information!")
                .ArgsAtMax(1)
                .IsHidden()
                .Do(AnimeTools.GetHBUser);

            group.CreateCommand("8ball")
                .WithPurpose("The magic eightball will answer all your doubts and questions!")
                .AnyArgs()
                .SecondDelay(20)
                .Do(Fun.EightBall);

            group.CreateCommand("lmao")
                .AnyArgs()
                .HourDelay(1)
                .Do(async e =>
                    await Tools.Reply(e, "https://www.youtube.com/watch?v=HTLZjhHIEdw"));

            group.CreateCommand("no")
                .SecondDelay(120)
                .AnyArgs()
                .Do(async e =>
                    await Tools.Reply(e, "pignig", false));

            group.CreateCommand("ayy")
                .MinuteDelay(2)
                .AnyArgs()
                .Do(Fun.Ayy);

            group.CreateCommand("commands")
                .AnyArgs()
                .IsHidden()
                .Do(Information.Commands);

            group.CreateCommand("img")
                .MinuteDelay(1)
                .ArgsAtLeast(1)
                .WithPurpose("Get an image of Google. (100 per day pls)")
                .Do(Fun.GetImageFromGoogleDotCom);

            group.CreateCommand("mala")
                .ArgsAtLeast(1)
                .WithPurpose("Find an anime of MAL and Link it together with it's synopsis.")
                .Do(AnimeTools.GetAnimeFromMAL);

            group.CreateCommand("malm")
                .ArgsAtLeast(1)
                .WithPurpose("Find a manga of MAL and Link it together with it's synopsis.")
                .Do(AnimeTools.GetMangaFromMAL);
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

            adminGroup.CreateCommand("kick")
                .WithPurpose("Only for super admins! Usage: `/admin kick {@username}`")
                .ArgsEqual(1)
                .Do(AdminCommands.KickUser);

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
                    if (Storage.programInfo.DevID == e.User.Id)
                        await RegularUsers.Save();
                });

            adminGroup.CreateCommand("clearperms")
                .MinPermissions(999)
                .Do(async e =>
                {
                    var serverInfo = Tools.GetServerInfo(e.Server.Id);
                    serverInfo.roleImportancy.Clear();
                    await Task.Delay(0);
                    Tools.SaveServerInfo();
                });
        }
        #endregion

    }
}
