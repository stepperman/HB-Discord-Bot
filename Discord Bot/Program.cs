using Discord;
using Newtonsoft.Json;
using System;
using Discord_Bot.CommandPlugin;
using System.Threading.Tasks;
using System.IO;

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
            _admincommands = new CommandsPlugin(client, null, '-');
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
                
                await Tools.Reply(e.User, client.GetChannel(server.welcomingChannel), $"Goodbye, **{e.User.Name}**. It was nice having you here.", false);
            };

            _client.UserBanned += async (s, e) =>
            {
                var server = Tools.GetServerInfo(e.Server.Id);

                if (server.welcomingChannel == 0)
                    return;

                await Tools.Reply(e.User, client.GetChannel(server.welcomingChannel), $"**{e.User.Name}** was fuckin banned lmao.", false);
            };


            _client.MessageReceived += async (s, e) =>
            {
                await Tools.OfflineMessage(e);
                await Modules.Games.AyyGame.Game(e);
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
                while (client.Status != UserStatus.Online)
                {
                    //Remove this?
                }
            }
        }



        #region New Users




        #endregion

        #region commands
        private static void BuildCommands(CommandGroupBuilder group)
        {
            group.DefaultMinPermissions(0);
            
            group.CreateCommand("normie")
                .Do(async e =>
                {
                    await Tools.Reply(e, "https://www.youtube.com/watch?v=JCeOf2q6_TA", false);
                });

            group.CreateCommand("source")
                .Do(async e =>
                {
                    await Tools.Reply(e, "Y-you want to see my source code..? A-a-alright then.. <https://github.com/stepperman/HB-Discord-Bot> ... baka hentai 😊");
                });

            //Added by Will (d0ubtless)
            group.CreateCommand("kazoo")
                .Do(async e =>
                {
                    await Tools.Reply(e, "You need the kazoo, if you can't take part in this episode, you're a fucking faggot, you should just go kill yourself https://youtu.be/g-sgw9bPV4A", false);
                });

            group.CreateCommand("hidechannel")
                .ArgsAtLeast(1)
                .IsHidden()
                .Do(Modules.HideChannel.Hide);

            group.CreateCommand("showchannel")
                .ArgsEqual(1)
                .IsHidden()
                .Do(Modules.HideChannel.Show);

            group.CreateCommand("listhiddenchannels")
                .NoArgs()
                .IsHidden()
                .Do(Modules.HideChannel.List);

            group.CreateCommand("hb")
                .WithPurpose("Find a User's HummingBird account with it's information!")
                .ArgsAtMax(1)
                .IsHidden()
                .Do(AnimeTools.GetHBUser);

            //Idea by Will but I perfected it and it's a fucking game now, who would've thought.
            group.CreateCommand("shoot")
                .DelayIsUnignorable()
                .SecondDelay(30)
                .WithPurpose("shoot a user, with a chance to miss! Usage: type /shoot and tag any amount of users you want.\n`/shoot stats` for your personal score\n`/shoot top` for the top 5 killers!")
                .Do(Modules.Games.ShootGame.ShootUser);

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
				
            //Added by Will (d0ubtless)
			group.CreateCommand("noice")
                .AnyArgs()
                .MinuteDelay(1)
                .Do(async e =>
                {
                    await Tools.Reply(e, "https://youtu.be/a8c5wmeOL9o");
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

            group.CreateCommand("hello")
                .AnyArgs()
                .HourDelay(1)
                .Do(async e =>
                {   
                    await Tools.Reply(e, $"Hello, {e.User.Mention}", false);
                });
				
            group.CreateCommand("ayy")
                .MinuteDelay(2)
                .AnyArgs()
                .Do(Fun.Ayy);

            group.CreateCommand("bullying")
                .AnyArgs()
                .WithPurpose("Getting bullied?")
                .IsHidden()
                .MinuteDelay(30)
                .Do(Fun.Bullying);

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

            group.CreateCommand("feedback")
                .WithPurpose("Give feedback to the bot! Stepper will read it sometime soon.. I think.")
                .ArgsAtLeast(1)
                .IsHidden()
                .Do(async e =>
                {
                    StreamWriter fs = new StreamWriter("../feedback.txt", true);
                    await fs.WriteLineAsync($"{e.User.Name} suggested: {e.ArgText}");
                    fs.Close();
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
            adminGroup.DefaultMinPermissions(0);
            
            adminGroup.CreateCommand("delete")
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
            adminGroup.CreateCommand("timeout")
                .WithPurpose("Time out someone. Usage: `/admin timeout {@username} {time in minutes}`.")
                .ArgsAtLeast(1)
                .Do(AdminCommands.TimeoutUser);
            adminGroup.CreateCommand("commands")
                .IsHidden()
                .AnyArgs()
                .Do(AdminCommands.GetCommands);
        }
        #endregion
        
    }
}
