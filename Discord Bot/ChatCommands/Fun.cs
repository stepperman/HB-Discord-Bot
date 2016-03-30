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
        private static Dictionary<ulong, uint> MostKills;
        private static string PathToKillScore = "../LocalFiles/killscore.json";

        static Fun()
        {
            if (!File.Exists(PathToKillScore))
            {
                Tools.CreateFile(PathToKillScore);
                MostKills = new Dictionary<ulong, uint>();
            }
            else
            {
                string json = Tools.ReadFile(PathToKillScore);
                MostKills = JsonConvert.DeserializeObject<Dictionary<ulong, uint>>(json);
                if(MostKills == null)
                    MostKills = new Dictionary<ulong, uint>();

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

                    if (msg.ToLower() != "ayy")
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
        /// If you shoot someone, you have chance to shoot yourself in a certain body part. (<seealso cref="BodyParts"/>)
        /// If that happens, you get timed out for the minutes it has. I think it's one of the funniest commands I've made yet for some reason.
        /// But well you can tag multiple! For every person you tag it increases the chance of hitting yourself by 5 procent.
        /// Or well, it decreases the chance of hitting them by 5 percent to better associate it as it is in the code.
        /// There's 2 commands with this as well, /shoot top and /shoot stats
        /// with /shoot stats you can see your own score. The amount of people you've brutally murdered you sick fuck.
        /// with /shoot top you can see the top 5 scores of other people. They're the most brutal murderers on this server!
        /// </summary>
        public static Func<CommandArgs, Task> ShootUser = async e =>
        {
            //Prematurely check if the user exists in the dictionary, if not, create the fuck.
            if (!MostKills.ContainsKey(e.User.Id))
                MostKills.Add(e.User.Id, 0);
            
            Console.WriteLine(MostKills[e.User.Id]);

            var arg = e.Args[0];
            if (arg == "stats")
            {
                uint score = MostKills[e.User.Id];
                await Tools.Reply(e, $"You killed {score} people.");
                return;
            }
            else if (arg == "top")
            {
                var list = MostKills.ToList();
                list.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                list.Reverse();

                string topresponse = "Top 5 players:";
                for (int i = 0; i < 5; i++)
                {
                    if (i == list.Count())
                        break;

                    User user = e.Server.GetUser(list[i].Key);
                    var userScore = list[i].Value;
                    topresponse += $"\n#{i+1}: **{user.Name}** with a killcount of {userScore}!";
                }

                await Tools.Reply(e, topresponse, false);
                return;
            }

            //Get count of all the mentioned users. Can be multiple. Count starts at one, an array starts at 0. So if you'd want to access
            //the first occurence in an array. You'd use e.Message.MentionedUsers[0].
            //Actually e.Message.MentionedUsers.ToArray()[0] because it's an IEnumerable but that's fuck.
            int mentionedUserCount = e.Message.MentionedUsers.Count();

            var chance = Tools.random.Next(151); // 0 to 100
            var hitChance = chance - (5 * mentionedUserCount);

            if (mentionedUserCount != 0 && (hitChance < 25 || e.Message.MentionedUsers.Contains(e.User)))
            {
                Array x = Enum.GetValues(typeof(BodyParts));
                var bodypart = x.GetValue(Tools.random.Next(x.Length));

                await Tools.Reply(e, $"Woops~! You just shot yourself in the {bodypart.ToString().ToLower()}! You've been timed out for {(int)bodypart} minutes! Your chance was {hitChance}. (need > 25 to murder)");
                await Program.timeout.TimeoutUser(e, (double)((int)bodypart), e.User);
                return;
            }

            //No mentioned users? Fuck off.
            if (mentionedUserCount > 0)
            {
                //Already create the premade response
                string response = $"{e.User.Mention} just shot ";


                if (mentionedUserCount != 1)
                {
                    for (int i = 0; i < mentionedUserCount; i++)
                    {
                        //Add the name to response.
                        response += e.Message.MentionedUsers.ToArray()[i].Mention;

                        //If this is the one to last mentioned user, add a " , ".
                        if (i == mentionedUserCount - 2)
                            response += " , ";
                        //Otherwise if it's less than the one to last mentioned user, add an " and ".
                        else if (i < mentionedUserCount - 2)
                            response += " and ";
                    }
                }

                //response.
                if (mentionedUserCount == 1)
                    await Tools.Reply(e, $"{response}{e.Message.MentionedUsers.ToArray()[0].Mention} to fucking death. Your chance was {chance} (need > 25)", false);
                else
                    await Tools.Reply(e, $"{response} to a fucking death. Your chance was {chance} (need > 25)", false);

                //aaand save the kills he has.
                Console.WriteLine(MostKills[e.User.Id]);
                MostKills[e.User.Id] += (uint)mentionedUserCount;
                Console.WriteLine(MostKills[e.User.Id]);

                //Serialize it so that it exists even after the bot is down.
                string json = JsonConvert.SerializeObject(MostKills);
                Tools.SaveFile(json, PathToKillScore, false); //Save it to disk.
            }
        };

        enum BodyParts
        {
            Foot = 1,
            Knee = 2,
            Leg = 3,
            Heart = 4,
            Head = 5
        }
    }
}
