using Discord;
using Newtonsoft.Json;
using System;
using Discord_Bot.CommandPlugin;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

/// <summary>
/// Perms:
/// > 1000 - Sabre and Stepper
/// > 100 - Mods
/// > 50 - Allow spamming
/// > 10 - Users
/// </summary>

namespace Discord_Bot
{
    class Program
    {
        private static DiscordClient _client;
        public static CommandsPlugin _commands, _admincommands;
        public static Timeout timeout;
        public static dynamic ProgramInfo = null;
        
        static void Main(string[] args)
        {
            var client = new DiscordClient();
            _client = client;
            _client.Log.Message += (s, e) => Console.WriteLine($"[{e.Severity}] {e.Source}: {e.Message}");
            
            _commands = new CommandsPlugin(client);
            _admincommands = new CommandsPlugin(client, (e, s) => { return Tools.GetPerms(s, e); }, '-');
            _commands.CreateCommandGroup("", group => BuildCommands(group));
            _admincommands.CreateCommandGroup("", adminGroup => BuildAdminCommands(adminGroup));
            
            //Get Programinfo
            if(File.Exists("./../LocalFiles/ProgramInfo.json"))
            {
                using (StreamReader sr = new StreamReader("./../LocalFiles/ProgramInfo.json"))
                {
                    var jsonfile = sr.ReadToEnd();
                    ProgramInfo = JsonConvert.DeserializeObject(jsonfile);
                    Console.WriteLine(ProgramInfo.username);
                }
            }

            _client.UserJoined += async (s, e) =>
            {
                await Information.NewUserText(e.User, e.Server);
                await Information.WelcomeUser(_client, e.User, e.Server.Id);
            };

            _client.UserLeft += async (s, e) =>
            {
                var server = Tools.GetServerInfo(e.Server.Id);

                if (server.welcomingChannel == 0)
                    return;
                
                await Tools.Reply(e.User, client.GetChannel(server.welcomingChannel), $"Goodbye, **{e.User.Mention}**. It was nice having you here.", false);
            };

            _client.UserBanned += async (s, e) =>
            {
                var server = Tools.GetServerInfo(e.Server.Id);

                if (server.welcomingChannel == 0)
                    return;

                await Tools.Reply(e.User, client.GetChannel(server.welcomingChannel), $"**{e.User.Mention}** was fuckin banned lmao.", false);
            };

            _client.ChannelCreated += async (s, e) =>
            {
                try
                {
                    var role = e.Server.FindRoles("qttimedout").FirstOrDefault();
                    await e.Channel.AddPermissionsRule(role, new ChannelPermissionOverrides(null, null, null, PermValue.Deny));
                }
                catch (Exception) {  }
            };

            _client.MessageReceived += async (s, e) =>
            {
                await Modules.Games.AyyGame.Game(e);
                await Modules.YouTube.ReceivedMessage(e.Message);

                if (e.Channel.IsPrivate)
                    return;

                if(Tools.GetServerInfo(e.Server.Id).RegularUsersEnabled)
                    await RegularUsers.ReceivedMessage(e);
            };

            _client.GatewaySocket.Disconnected += async (s, e) =>
            {
                while(_client.State != ConnectionState.Connected)
                {
                    try
                    {
                        await _client.Connect(ProgramInfo.username, ProgramInfo.password);
                    }
                    catch(Exception ex)
                    {
                        Tools.LogError("Couldn't connect!", ex.Message);
                    }

                    await Task.Delay(30000);
                }
            };

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
            try
            {
                _client.ExecuteAndWait(async () =>
                {
                    var username = ProgramInfo.username;
                    var password = ProgramInfo.password;
                    var nottoken = Convert.ToString(ProgramInfo.bot_token);

                    await _client.Connect(nottoken);
                    timeout = new Timeout(_client);
                    Storage.client = _client;
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
                {
                    await Tools.Reply(e, "Here's my source code! <https://github.com/stepperman/HB-Discord-Bot>");
                });

            group.CreateCommand("yt").Alias("youtube")
                .ArgsAtLeast(1)
                .WithPurpose("Find YouTube videos!")
                .Do(Discord_Bot.Modules.YouTube.FindYouTubeVideo);

            group.CreateCommand("uptime")
                .Do(Uptime.ShowUptime)
                .WithPurpose("show the bot's uptime.");

            group.CreateCommand("downtime")
                .Do(async e =>
                {
                    await Tools.Reply(e, "a **long time.**");
                });

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
                {
                    await Tools.Reply(e, "https://www.youtube.com/watch?v=HTLZjhHIEdw");
                });
				
            group.CreateCommand("no")
                .SecondDelay(120)
                .AnyArgs()
                .Do(async e =>
                {
                    await Tools.Reply(e, "pignig", false);
                });

            //Added by Will (d0ubtless)
            group.CreateCommand("codekeem")
                .SecondDelay(120)
                .AnyArgs()
                .Do(async e =>
                {
                    await e.Channel.SendFile("keemstar.png");
                    await Tools.Reply(e, "You have used code 'KEEM'", true);
                });
				
            group.CreateCommand("ayy")
                .MinuteDelay(2)
                .AnyArgs()
                .Do(Fun.Ayy);

            group.CreateCommand("commands")
                .AnyArgs()
                .IsHidden()
                .Do(Information.Commands);

            group.CreateCommand("help")
                .WithPurpose("Show the getting-started guide!")
                .AnyArgs()
                .IsHidden()
                .Do(async e =>
                {
                    await Information.NewUserText(e.User, e.Server);
                });
				
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

            adminGroup.CreateCommand("welcomeme")
                .Do(async e =>
                {
                    await Tools.Reply(e, Information.GetWelcomeReplies()[Tools.random.Next(Information.GetWelcomeReplies().Length)], false);
                });

            adminGroup.CreateCommand("commands")
                .IsHidden()
                .AnyArgs()
                .Do(AdminCommands.GetCommands);

            adminGroup.CreateCommand("save")
                .Do(async e =>
                {
                    if (ProgramInfo.DevID == e.User.Id)
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
