using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Discord_Bot.CommandPlugin;
using Discord;
using Newtonsoft.Json;

namespace Discord_Bot
{
    static class Fun
    {
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

                Storage.ayyscore++;
                Tools.SaveServerInfo();

                string text = "get as long a chain of /ayy 's before it gets broken. High Score: {0} Current Score: {1}";
                await e.Channel.SendMessage("ayy");
                await Task.Delay(100);
                await e.Channel.Edit(e.Channel.Name, String.Format(text, info.ayyHighScore, Storage.ayyscore), e.Channel.Position);
            }
            else
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
                    client.QueryString.Add("safe", Tools.GetServerInfo(e.Server.Id).safesearch);
                    client.QueryString.Add("num", "10");
                    try
                    {
                        response = await client.DownloadStringTaskAsync("https://www.googleapis.com/customsearch/v1");
                    }
                    catch (System.Net.WebException ex)
                    {
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            await Tools.Reply(e, "The daily limit has been reached. Try again tomorrow!");
                            return;
                        }
                        else
                        {
                            await Tools.Reply(e, $"Error: {ex.Message}");
                            return;
                        }
                    }
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

        public static Func<CommandArgs, Task> UrbanDictionary = async e =>
        {
            using (WebClient client = new WebClient())
            {
                string response = await client.DownloadStringTaskAsync($"http://api.urbandictionary.com/v0/define?term={e.ArgText}");
                dynamic json = JsonConvert.DeserializeObject(response);

                if (json.result_type.ToString() == "no_results")
                {
                    await Tools.Reply(e, $"Could not find the definition of {e.ArgText}");
                    return;
                }

                string message = $"\nDefinition of {e.ArgText}:\n```{json.list[0].definition.ToString()}```\n\nExample:\n```{json.list[0].example.ToString()}```\n";
                message += $"Permalink: <http://www.urbandictionary.com/define.php?term={WebUtility.UrlEncode(e.ArgText)}>";
                await Tools.Reply(e, message);
            }
        };
    }
}
