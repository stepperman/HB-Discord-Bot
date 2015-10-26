using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Discord_Bot.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Discord_Bot
{
    class Program
    {
        private static DiscordClient _client;
        private static CommandsPlugin _commands;

        private static List<User> Admins = new List<User>();

        static void Main(string[] args)
        {

            var client = new DiscordClient();

            _client = client;
            ServicePointManager
    .ServerCertificateValidationCallback +=
    (sender, cert, chain, sslPolicyErrors) => true;
            _client.LogMessage += (s, e) => Console.WriteLine($"[{e.Severity}] {e.Source}: {e.Message}");

            _commands = new CommandsPlugin(client);
            _commands.CreateCommandGroup("", group => BuildCommands(group));

            _client.UserAdded += async (s, e) =>
            {
                await _client.SendMessage(_client.GetChannel("99341276532449280"), $"Holy shit! A new user! Welcome {e.User.Name}");
            };

            _commands.CommandError += (s, e) =>
            {
                var ex = e.Exception.GetBaseException();
                if (ex is PermissionException)
                    Reply(e, "Sorry, you do not have the permissions to use this command!");
                else
                    Reply(e, $"Error: {ex.Message}.");

            };

            _client.Run(async () =>
            {
				//Logs in with email and password.
                await _client.Connect("hidden", "hidden");

                Server Commons = _client.GetServer("99333280020566016");

                var God = _client.FindRoles(Commons, "The Lone Wanderer").FirstOrDefault();
                var Demigod = _client.FindRoles(Commons, "Demigod").FirstOrDefault();
                var RoyalGuard = _client.FindRoles(Commons, "Royal Guard").FirstOrDefault();
                var Commander = _client.FindRoles(Commons, "Commander").FirstOrDefault();

                foreach (var god in God.Members)
                    Admins.Add(god);

                foreach (var demiGod in Demigod.Members)
                    Admins.Add(demiGod);

                foreach (var royalGuard in RoyalGuard.Members)
                    Admins.Add(royalGuard);

                foreach (var commander in Commander.Members)
                    Admins.Add(commander);
            });
        }

        private static Random random = new Random();
        private static void BuildCommands(CommandGroupBuilder group)
        {
            group.DefaultMinPermissions(0);

            group.CreateCommand("img")
                .WithPurpose("Get a random image pulled from Google!")
                .AnyArgs()
                .IsHidden()
                .Do(async e =>
                {
                    var client = new HttpClient();
                    string uri = $"https://ajax.googleapis.com/ajax/services/search/images?";
                    NameValueCollection values = HttpUtility.ParseQueryString(string.Empty);
                    values.Add("v", "1.0");
                    values.Add("q", e.ArgText);
                    values.Add("rsz", "8");
                    values.Add("start", random.Next(1, 12).ToString());
                    values.Add("safe", "active");



                    try
                    {
                        var response = await Get(uri, values);
                        var data = JObject.Parse(response);
                        List<string> images = new List<string>();
                        foreach (var element in data["responseData"]["results"])
                        {
                            var image = element["unescapedUrl"];
                            images.Add(image.ToString());
                        }

                        var imageURL = images[random.Next(images.Count)].ToString();
                        Console.WriteLine(imageURL);
                        await Reply(e, $"{e.ArgText} : {imageURL}");
                    }
                    catch (Exception ex)
                    {
                        await Reply(e, $"{ex.Message}");
                    }
                });
            group.CreateCommand("hb")
                .WithPurpose("Find a User's HummingBird account with it's information!")
                .ArgsAtMax(1)
                .IsHidden()
                .Do(async e =>
                {
                    var client = new HttpClient();
                    string url = $"http://hummingbird.me/api/v1/users/{e.ArgText}";
                    string userUrl = $"http://hummingbird.me/users/{e.ArgText}";

                    try
                    {
                        string response = await client.GetStringAsync(url);
                        var json = JObject.Parse(response);

                        var username = json["name"].ToString();
                        var waifu = json["waifu"].ToString();
                        var waifu_prefix = json["waifu_or_husbando"].ToString();
                        var avatar = json["avatar"].ToString();
                        var about = json["about"].ToString();
                        var bio = json["bio"].ToString();
                        var location = json["location"].ToString();
                        var website = json["website"].ToString();
                        var life_spent_on_anime = Int32.Parse(json["life_spent_on_anime"].ToString());

                        string lifeAnime = CalculateTime(life_spent_on_anime);

                        string messageToPost = $@"
**User**: {username}
**Avatar**: {avatar} 
**{waifu_prefix}**: {waifu}
**Bio:** {bio}
**Time wasted on Anime:** {lifeAnime}";

                        if (String.IsNullOrWhiteSpace(location))
                            messageToPost += $"\n**Location:** {location}";
                        if (String.IsNullOrWhiteSpace(website))
                            messageToPost += $"\n**Website:** {website}";

                        messageToPost += $"\n{userUrl}";

                        await Reply(e, messageToPost);

                    }
                    catch (Exception ex)
                    {
                        await Reply(e, $"Error: {ex.Message}");
                    }

                });
            group.CreateCommand("8ball")
                .WithPurpose("The magic eightball will answer all your doubts and questions!")
                .AnyArgs()
                .MinuteDelay(3)
                .Do(async e =>
                {
                    string[] responses = { "Not so sure", "Most likely", "Absolutely not", "Outlook is good", "Never",
"Negative", "Could be", "Unclear, ask again", "Yes", "No", "Possible, but not probable" };
                    string response;


                    if (e.ArgText.Length != 0)
                        response = "I can't do anything with empty prompts.";
                    if (e.ArgText[e.ArgText.Length - 1] != '?')
                        response = "Please end your sentence with a question mark appropriately.";
                    else
                        response = responses[random.Next(responses.Length)];
                    
                    await Reply(e, response);
                });
            group.CreateCommand("ayy")
                .HourDelay(1)
                .AnyArgs()
                .Do(async e =>
                {
                    await Reply(e, "ayy", false);
                });
            group.CreateCommand("lmao")
                .AnyArgs()
                .HourDelay(1)
                .Do(async e =>
                {
                    await Reply(e, "https://www.youtube.com/watch?v=HTLZjhHIEdw");
                });
            group.CreateCommand("no")
                .SecondDelay(120)
                .AnyArgs()
                .Do(async e =>
                {
                    await Reply(e, "pignig", false);
                });
            group.CreateCommand("hello")
                .AnyArgs()
                .HourDelay(1)
                .Do(async e =>
                {   
                    await Reply(e, $"Hello, {Mention.User(e.User)}", false);
                });
            group.CreateCommand("bullying")
                .AnyArgs()
                .WithPurpose("Getting bullied?")
                .IsHidden()
                .MinuteDelay(30)
                .Do(async e =>
                {
                    var OnlineAdmins = new List<User>();

                    foreach (var admin in Admins)
                        if (admin.Status == UserStatus.Online || admin.Status == "idle")
                            OnlineAdmins.Add(admin);

                    User toMention;
                    if (OnlineAdmins.Count != 0)
                        toMention = OnlineAdmins[random.Next(OnlineAdmins.Count)];
                    else
                        toMention = Admins[random.Next(Admins.Count)];

                    await _client.SendFile(e.Channel, "antibully.jpg");
                    await Reply(e, $"{Mention.User(toMention)} **BULLYING IN PROGESS :: {e.User.Name.ToUpper()} IS BEING BULLIED** ", false);
                    await Task.Delay(300);
                    await Reply(e, $"{Mention.User(toMention)} **BULLYING IN PROGESS :: {e.User.Name.ToUpper()} IS BEING BULLIED** ", false);
                });
            group.CreateCommand("commands")
                .AnyArgs()
                .IsHidden()
                .Do(async e =>
                {
                    string response = $"The character to use a command right now is '{_commands.CommandChar}'.\n";
                    foreach(var cmd in _commands._commands)
                    {
                        if(!String.IsNullOrWhiteSpace(cmd.Purpose))
                        {
                            response += $"**{cmd.Parts[0]}** - {cmd.Purpose}";

                            if (cmd.CommandDelay == null)
                                response += "\n";
                            else
                                response += $" **|** Time limit: once per {cmd.CommandDelayNotify} {cmd.timeType}.\n";
                        }
                    }
                    
                    await _client.SendPrivateMessage(e.User, response);
                });
            group.CreateCommand("admin delete")
                .ArgsEqual(1)
                .Do(async e =>
                {
                    if (!canUseAdminCommands(e.User))
                        return;
                    
                    int deleteNumber = 0;

                    Int32.TryParse(e.Args[0], out deleteNumber);

                    var messages = await _client.DownloadMessages(e.Channel, deleteNumber);

                    await _client.DeleteMessages(messages);
                    await Reply(e, $"just deleted {deleteNumber} messages on this channel!");
                });
            group.CreateCommand("admin kick")
                .ArgsEqual(1)
                .Do(async e =>
                {
                    if (!canUseAdminCommands(e.User))
                    {
                        await Reply(e, "You're not allowed to kick people!");
                        return;
                    }

                    string userID = e.Args[0];

                    userID = userID.Substring(1);

                    var Users = _client.FindUsers(e.Server, userID);
                    var userTokick = Users.FirstOrDefault();

                    if (userTokick == null)
                        return;

                    if (userTokick == _client.CurrentUser)
                        return;

                    await _client.SendPrivateMessage(userTokick, $"You've been kicked by {e.User.Name}, you can rejoin by using this url: https://discord.gg/0YOrPxx9u1wtJE0B");
                    await Reply(e, $"just kicked {userTokick.Name}!");
                    await _client.KickUser(userTokick);
                });
        }
        
        protected static bool canUseAdminCommands(User user)
        {
            bool canDelete = false;

            foreach (var admin in Admins)
            {
                if (user == admin)
                {
                    canDelete = true;
                    break;
                }
            }

            return canDelete;
        }

        protected static string CalculateTime(int minutes)
        {
            if (minutes == 0)
                return "No time.";

            int years, months, days, hours = 0;

            hours = minutes / 60;
            minutes %= 60;
            days = hours / 24;
            hours %= 24;
            months = days / 30;
            days %= 30;
            years = months / 12;
            months %= 12;

            string animeWatched = "";

            if(years > 0)
            {
                animeWatched += years;
                if (years == 1)
                    animeWatched += " **year**";
                else
                    animeWatched += " **years**";
            }

            if(months > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += ", ";
                animeWatched += months;
                if (months == 1)
                    animeWatched += " **month**";
                else
                    animeWatched += " **months**";
            }

            if(days > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += ", ";
                animeWatched += days;
                if (days == 1)
                    animeWatched += " **day**";
                else
                    animeWatched += " **days**";
            }

            if(hours > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += ", ";
                animeWatched += hours;
                if (hours == 1)
                    animeWatched += " **hour**";
                else
                    animeWatched += " **hours**";
            }

            if(minutes > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += " and ";
                animeWatched += minutes;
                if (minutes == 1)
                    animeWatched += " **minute**";
                else
                    animeWatched += " **minutes**";
            }

            return animeWatched;


        }

        protected static async Task<string> Get(string url, NameValueCollection values)
        {
            string hello = String.Empty;
            var client = new HttpClient();


            url += values.ToString();

            hello = await client.GetStringAsync(url);

            return hello;
        }



        protected static async Task<string> Post(string Uri, Dictionary<string, string> values)
        {
            string response;
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(values);

            var request = await client.PostAsync(Uri, content);

            response = request.ToString();

            return response;

        }

        protected static async Task Reply(User user, Channel channel, string text, bool mentionUser)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (channel.IsPrivate || !mentionUser)
                    await _client.SendMessage(channel, text);
                else
                    await _client.SendMessage(channel, $"{Mention.User(user)}: {text}");
            }
        }
        protected static Task Reply(CommandArgs e, string text)
            => Reply(e.User, e.Channel, text, true);
        protected static Task Reply(CommandArgs e, string text, bool mentionUser)
            => Reply(e.User, e.Channel, text, mentionUser);

    }
}
