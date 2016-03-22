using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord_Bot.Commands;
using Discord;
using Newtonsoft.Json;

namespace Discord_Bot
{
    class Fun
    {
        private static int ayyscore = 0;

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
    }
}
