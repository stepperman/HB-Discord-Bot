using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Discord_Bot.Commands;
using Discord;
using Newtonsoft.Json;

namespace Discord_Bot
{
    static class Fun
    {
        private static int ayyscore = 0;
        private static Dictionary<ulong, ShootPlayer> MostKills;
        private static string PathToKillScore = "../LocalFiles/killscore.json";

        static Fun()
        {
            if (!File.Exists(PathToKillScore))
            {
                Tools.CreateFile(PathToKillScore);
                MostKills = new Dictionary<ulong, ShootPlayer>();
            }
            else
            {
                string json = Tools.ReadFile(PathToKillScore);
                MostKills = JsonConvert.DeserializeObject<Dictionary<ulong, ShootPlayer>>(json);
                if(MostKills == null)
                    MostKills = new Dictionary<ulong, ShootPlayer>();

            }
        }

        public static Func<CommandArgs, Task> EightBall = async e =>
        {
            string[] responses = {  "It is certain",
                                        "It is decidedly so",
                                        "Without a doubt",
                                        "Yes, definitely",
                                        "You may rely on it",
                                        "As I see it, yes",
                                        "Most likely",
                                        "Outlook good",
                                        "Yes",
                                        "Signs point to yes",
                                        "Reply hazy try again",
                                        "Ask again later",
                                        "Better not tell you now",
                                        "Cannot predict now",
                                        "Concentrate and ask again",
                                        "Don't count on it",
                                        "My reply is no",
                                        "My sources say no",
                                        "Outlook not so good" };

            string response;


            if (e.ArgText.Length == 0)
                response = "I can't do anything with empty prompts.";
            else if (e.ArgText[e.ArgText.Length - 1] != '?')
                response = "Please end your sentence with a question mark appropriately.";
            else
                response = responses[Tools.random.Next(responses.Length)];

            await Tools.Reply(e, response);
        };

        public static Func<CommandArgs, Task> Ayy = async e =>
        {
            if (e.Channel.Id == 134267667245694976)
            {
                var info = Tools.GetServerInfo(e.Server.Id);

                ayyscore++;
                if (ayyscore > info.ayyScore)
                {
                    info.ayyScore = ayyscore;
                }

                Tools.SaveServerInfo();

                string text = "get as long a chain of /ayy 's before it gets broken. High Score: {0} Current Score: {1}";

                await e.Channel.Edit(e.Channel.Name, String.Format(text, info.ayyScore, ayyscore), e.Channel.Position);
            }

            await Tools.Reply(e, "ayy", false);
        };

        public static Func<CommandArgs, Task> Bullying = async e =>
        {
            var Admins = new List<User>();
            var OnlineAdmins = new List<User>();

            var info = Tools.GetServerInfo(e.Server.Id);

            foreach (var importantrole in info.roleImportancy.Keys)
            {
                Role role = null;
                foreach (var rol in e.Server.Roles)
                {
                    if (rol.Id.ToString() == importantrole)
                    {
                        role = rol;
                        break;
                    }
                }

                foreach (var u in e.Server.Users)
                {
                    if (role != null)
                        if (u.HasRole(role))
                            Admins.Add(u);
                }
            }

            foreach (var admin in Admins)
                if (admin.Status == UserStatus.Online || admin.Status == "idle")
                    OnlineAdmins.Add(admin);

            User toMention = null;
            if (OnlineAdmins.Count != 0)
                toMention = OnlineAdmins[Tools.random.Next(OnlineAdmins.Count)];
            else if (Admins.Count != 0)
                toMention = Admins[Tools.random.Next(Admins.Count)];

            await e.Channel.SendFile("antibully.jpg");
            await Tools.Reply(e, $"{toMention.Mention} **BULLYING IN PROGESS :: {e.User.Name.ToUpper()} IS BEING BULLIED** ", false);
            await Task.Delay(300);
            await Tools.Reply(e, $"{toMention.Mention} **BULLYING IN PROGESS :: {e.User.Name.ToUpper()} IS BEING BULLIED** ", false);
        };

        public static Func<CommandArgs, Task> GetImageFromGoogleDotCom = async e =>
        {
            try
            {
                string want = e.ArgText;
                string response = String.Empty;

                using (WebClient client = new WebClient())
                {
                    client.QueryString.Add("searchType", "image");
                    client.QueryString.Add("q", Uri.EscapeDataString(want));
                    client.QueryString.Add("key", Uri.EscapeDataString((string)Program.ProgramInfo.google_key_code));
                    client.QueryString.Add("cx", Uri.EscapeDataString((string)Program.ProgramInfo.google_cx_code));
                    client.QueryString.Add("safe", "medium");
                    client.QueryString.Add("num", "10");
                    response = await client.DownloadStringTaskAsync("https://www.googleapis.com/customsearch/v1");
                }

                if (response == String.Empty)
                {
                    await Tools.Reply(e, "No response. Servers might be down.");
                    return;
                }

                dynamic json = JsonConvert.DeserializeObject(response);
                var count = Enumerable.Count(json.items);
                if (count > 0)
                {
                    var rand = Tools.random.Next(0, count);
                    string link = json.items[rand].link;
                    await Tools.Reply(e, link);
                }
                else
                {
                    await Tools.Reply(e, "No items found :(.");
                }
            }
            catch (Exception ex)
            {
                Tools.LogError("[img]", ex.Message);
                await Tools.Reply(e, $"Error: {ex.Message}");
            }
        };
        
        public static async Task AyyGame(MessageEventArgs e)
        {
            if (e.Channel.Id == 134267667245694976 && ayyscore > 0)
            {
                try
                {
                    string msg = e.Message.Text;
                    if (msg[0] == '/')
                        msg = msg.Substring(1);

                    if (!msg.ToLower().Replace(" ", "").EndsWith("ayy"))
                    {
                        var info = Tools.GetServerInfo(e.Server.Id);
                        string text = "get as long a chain of /ayy 's before it gets broken. High Score: {0} Current Score: {1}";
                        var oldayy = ayyscore;
                        ayyscore = 0;
                        await Tools.Reply(e.User, e.Channel, $"You failed! The chain has been broken. The score was: {oldayy}", true);
                        await e.Channel.Edit(e.Channel.Name, String.Format(text, info.ayyScore, ayyscore));
                    }
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Alright so how this game works is that you can shoot a user, or multiple users.
        /// If you shoot someone, you have chance to shoot yourself in a certain body part. (<seealso cref="BodyPart"/>)
        /// If that happens, you get timed out for the minutes it has. I think it's one of the funniest commands I've made yet for some reason.
        /// But well you can tag multiple! For every person you tag it increases the chance of hitting yourself by 5 procent.
        /// Or well, it decreases the chance of hitting them by 5 percent to better associate it as it is in the code.
        /// There's 2 commands with this as well, /shoot top and /shoot stats
        /// with /shoot stats you can see your own score. The amount of people you've brutally murdered you sick fuck.
        /// with /shoot top you can see the top 5 scores of other people. They're the most brutal murderers on this server!
        /// 
        /// TODO: Add death counter.
        /// TODO: Add Kill/Death ratio.
        /// </summary>
        public static Func<CommandArgs, Task> ShootUser = async e =>
        {
            //Prematurely check if the user exists in the dictionary, if not, create the fuck.
            if (!MostKills.ContainsKey(e.User.Id))
                MostKills.Add(e.User.Id, new ShootPlayer());

            //check if the mentioned users do not exists, if not, create the fuck(s)
            if (e.Message.MentionedUsers.Count() > 1)
            {
                foreach (var user in e.Message.MentionedUsers)
                {
                    if (!MostKills.ContainsKey(user.Id))
                        MostKills.Add(user.Id, new ShootPlayer());
                }
            }
            
            Console.WriteLine(MostKills[e.User.Id]);

            var arg = e.Args[0];

            //Get your own score
            //TODO: Tag users to get their score. (1 user or more?)
            if (arg == "stats")
            {
                if (e.Message.MentionedUsers.Count() > 0)
                {
                    var user = e.Message.MentionedUsers.ToArray()[0];
                    var sc = MostKills[user.Id].kills;
                    var de = MostKills[user.Id].deaths;
                    var k = MostKills[user.Id].kdRatio;

                    await Tools.Reply(e, $"{user.Name} has killed {sc} people, died {de} times. Their k/d ratio is {k}");

                    return;
                }

                var score = MostKills[e.User.Id].kills;
                var deaths = MostKills[e.User.Id].deaths;
                var kd = MostKills[e.User.Id].kdRatio;

                await Tools.Reply(e, $"You've killed {score} people, and you've died {deaths} times. Your k/d ratio is {kd}");
                return;
            }

            //Get top players
            else if (arg == "top")
            {
                var list = ShootTopPlayers(5);

                string players = "Top 5 murderers:\n";

                int i = 1;
                foreach (var element in list)
                {
                    var username = e.Server.GetUser(element.Key).Name;
                    var userKills = element.Value.kills;
                    var userDeaths = element.Value.deaths;
                    var kd = element.Value.kdRatio;

                    players += $"#{i}: **{username}** Kills: {userKills}. Deaths: {userDeaths}. k/d ratio: {kd}\n";
                    i++;
                }

                await Tools.Reply(e, players);
                return;

            }

            //Get count of all the mentioned users. Can be multiple. Count starts at one, an array starts at 0. So if you'd want to access
            //the first occurence in an array. You'd use e.Message.MentionedUsers[0].
            //Actually e.Message.MentionedUsers.ToArray()[0] because it's an IEnumerable but that's fuck.
            int mentionedUserCount = e.Message.MentionedUsers.Count();
            
            //All the responses. {0} is the shooter, {1} the victim
            string[] responses =
            {
                "{0} just killed {1} to death.",
                "{0} just fucking murdered {1}.",
                "{0} exploded {1}.",
                "{1} is now dead. {0} didn't do it, I swear.",
                "{0} test fired his gun. The bullet ricochet to {1}. Woops.",
                "{0} assassinated {1}.",
                "{0} sprayed {1}.",
                "{0} no-scoped {1} to another dimension.",
                "{0} shot {1} to a fucking pulp.",
                "{0} didn't shoot {1} to death, but fucked them to death!",
                "I don't even want to say what {0} did to {1}.",
                "{0} quickscoped {1}.",
                "{0} invited {1} to 1v1 Rust. He won.",
                "{1} killed himself after years of bullying by {0}.",
                "{0} locked up {1} in a cell and forgot to feed him.",
                "{0} ripped {1}.",
                "{0} talked to {1}, and {1} died.",
                "{0} had a nice bath with {1}.",
                "{0} installed Windows 10 on {1}'s computer",
                "{1} has no idea what this has to do with the /shoot command that {0} initiated.", //courtesy to Will :*
                "{1} proposed to {0}. They said no." 
            };

            //Bodyparts
            Dictionary<string, double> BodyParts = new Dictionary<string, double>()
            {
                { "Foot", 0.5 },
                { "Stomach", 1 },
                { "Heart", 1.5 },
                { "Head", 2 }
            };

            //Chance needed
            double SUICIDE_CHANCE = 12.5;
            double MISS_CHANCE = 25;
            double SUICIDE_CHANCE_TOP = 25;
            double MISS_CHANCE_TOP = 50;
            int MAX_RAND = 100;

            double suicideChance = SUICIDE_CHANCE;
            double missChance = MISS_CHANCE;

            var chance = Tools.random.Next(0, MAX_RAND + 1);
            var hitChance = chance - (2 * mentionedUserCount);

            //If player is in top 5, set the hit chance to be a harder difficulty.
            if (ShootTopPlayers(5).Any(u => u.Key == e.User.Id))
            {
                suicideChance = SUICIDE_CHANCE_TOP;
                missChance = MISS_CHANCE_TOP;
            }

            if (mentionedUserCount == 0)
                return;

            bool shotHimself = e.Message.MentionedUsers.Contains(e.User);

            //Already create the premade response
            string names = $"";

            if (mentionedUserCount != 1)
            {
                for (int i = 0; i < mentionedUserCount; i++)
                {
                    //Add the name to response.
                    names += e.Message.MentionedUsers.ToArray()[i].Mention;

                    //If this is the one to last mentioned user, add a " , ".
                    if (i == mentionedUserCount - 2)
                        names += " and ";
                    //Otherwise if it's less than the one to last mentioned user, add an " and ".
                    else if (i < mentionedUserCount - 2)
                        names += ", ";
                }
            }
            else
                names = e.Message.MentionedUsers.ToArray()[0].Mention;

            //Suicide
            if (mentionedUserCount != 0 && (hitChance < suicideChance || shotHimself)) 
            {
                var bodypart = BodyParts.ElementAt(Tools.random.Next(BodyParts.Count));

                string s = bodypart.Value == 1.0 ? "s" : "";

                if (shotHimself)
                    await Tools.Reply(e, $"Dude! You just fucking shot yourself in the {bodypart.Key.ToLower()}! Why would you do that? You've been timed out for {bodypart.Value} minute{s}!");
                else
                    await Tools.Reply(e, $"Woops~! You just shot yourself in the {bodypart.Key.ToLower()}! You've been timed out for {bodypart.Value} minute{s}! Your chance was {hitChance}. (need > {missChance}/100)");

                //Save the suicide to the deaths
                MostKills[e.User.Id].deaths += 1;

                //Serialize it so that it exists even after the bot is down.
                string json = JsonConvert.SerializeObject(MostKills);
                Tools.SaveFile(json, PathToKillScore, false); //Save it to disk.

                await Program.timeout.TimeoutUser(e, bodypart.Value, e.User);
                return;
            }
            //Missed shot.
            else if (Tools.InRange(hitChance, suicideChance, missChance))
            {
                if (shotHimself)
                    await Tools.Reply(e, $"Wow! You almost shot yourself to death! For some reason, you missed. (need > {missChance}/100)");
                else
                    await Tools.Reply(e, $"{e.User.Mention} missed {names}. Your chance was {hitChance}. (need > {missChance}/100)", false);

                return;
            }
            //If not any of that, it's a hit!
            else
            {
                string response = "";

                //This is to cheat the system so that Aowashi always has a bath. Always.
                if (e.Message.MentionedUsers.Any(u => u.Id == 99511799421861888))
                    response = "{0} had a nice bath with {1}.".Replace("{0}", e.User.Mention).Replace("{1}", names);
                else
                    response = responses[Tools.random.Next(responses.Length)].Replace("{0}", e.User.Mention).Replace("{1}", names) ;

                //response.
                await Tools.Reply(e, $"{response} Your chance was {chance} (need > {missChance}/100)", false);

                //aaand save the kills he has.
                MostKills[e.User.Id].kills += (uint)mentionedUserCount;

                //Save the deaths of the people that the user has killed.
                foreach (var user in e.Message.MentionedUsers)
                {
                    MostKills[user.Id].deaths += 1;
                }

                //Serialize it so that it exists even after the bot is down.
                string json = JsonConvert.SerializeObject(MostKills);
                Tools.SaveFile(json, PathToKillScore, false); //Save it to disk.
            }
        };

        private static Dictionary<ulong, ShootPlayer> ShootTopPlayers(int amount)
        {
            var list = MostKills.ToList();
            list.Sort((pair1, pair2) => pair1.Value.kills.CompareTo(pair2.Value.kills));
            list.Reverse();
            return list.Take(amount).ToDictionary(x => x.Key, x => x.Value);
        }

        class ShootPlayer
        {
            public uint kills;
            public uint deaths;
            public decimal kdRatio
            {
                get
                {
                    if (kills == 0 || deaths == 0)
                        return -1;

                    return Math.Round((decimal)kills / (decimal)deaths, 3);
                }
            }
        }
        
    }
}
