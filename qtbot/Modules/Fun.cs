using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using qtbot.CommandPlugin;
using qtbot.BotTools;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Net.Http;

namespace qtbot.Modules
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
            else
                response = responses[Tools.random.Next(responses.Length)];

            await Tools.ReplyAsync(e, response);
        };

        public static Func<CommandArgs, Task> Ayy = async e =>
        {
            if (e.Channel.Id == 134267667245694976)
            {
                var info = Tools.GetServerInfo(e.Guild.Id);

                Storage.ayyscore++;
                Tools.SaveServerInfo();

                string text = "get as long a chain of /ayy 's before it gets broken. High Score: {0} Current Score: {1}";
                await e.Channel.SendMessageAsync("ayy");
                await Task.Delay(100);
                await e.Channel.ModifyAsync(options =>
                {
                    options.Topic = String.Format(text, info.ayyHighScore, Storage.ayyscore);
                });
                //e.Channel.Name, String.Format(text, info.ayyHighScore, Storage.ayyscore), e.Channel.Position
            }
            else
                await Tools.ReplyAsync(e, "ayy", false);
        };

        public static Func<CommandArgs, Task> Bullying = async e =>
        {
            var Admins = new List<SocketUser>();
            var OnlineAdmins = new List<SocketUser>();

            var info = Tools.GetServerInfo(e.Guild.Id);

            foreach (var importantrole in info.roleImportancy.Keys)
            {
                SocketRole role = null;
                foreach (var rol in e.Guild.Roles)
                {
                    if (rol.Id == importantrole)
                    {
                        role = rol;
                        break;
                    }
                }

                foreach (var u in e.Guild.Users)
                {
                    if (role != null)
                        if (u.RoleIds.Contains(role.Id))
                            Admins.Add(u);
                }
            }

            foreach (var admin in Admins)
                if (admin.Status == UserStatus.Online || admin.Status == UserStatus.Invisible ||
                admin.Status == UserStatus.Idle || admin.Status == UserStatus.AFK)
                    OnlineAdmins.Add(admin);

            SocketUser toMention = null;
            if (OnlineAdmins.Count != 0)
                toMention = OnlineAdmins[Tools.random.Next(OnlineAdmins.Count)];
            else if (Admins.Count != 0)
                toMention = Admins[Tools.random.Next(Admins.Count)];

            await e.Channel.SendFileAsync("antibully.jpg");
            await Tools.ReplyAsync(e, $"{toMention.Mention} **BULLYING IN PROGESS :: {e.Author.Mention} IS BEING BULLIED** ", false);
            await Task.Delay(300);
            await Tools.ReplyAsync(e, $"{toMention.Mention} **BULLYING IN PROGESS :: {e.Author.Mention} IS BEING BULLIED** ", false);
        };

        public static Func<CommandArgs, Task> GetImageFromGoogleDotCom = async e =>
        {
            QtNetHelper.QtNet qtNet = new QtNetHelper.QtNet("https://www.googleapis.com/customsearch/v1/");

                qtNet.Query = new Dictionary<string, string>
                {
                    { "searchType", "image" },
                    { "q", Uri.EscapeUriString(e.ArgText) },
                    { "key", Uri.EscapeUriString((string)Storage.programInfo.google_key_code) },
                    { "cx", Uri.EscapeUriString((string)Storage.programInfo.google_cx_code) },
                    { "safe", Tools.GetServerInfo(e.Guild.Id).safesearch },
                    { "num", "10" }
                };
                

                try
                {
                    dynamic json = JsonConvert.DeserializeObject(await qtNet.GetStringAsync());
                    string link = json.items[Tools.random.Next(0, Enumerable.Count(json.items))].link;
                    await Tools.ReplyAsync(e, link);
                }
                catch (System.Net.WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        await Tools.ReplyAsync(e, "The daily limit has been reached. Try again tomorrow!");
                        return;
                    }
                    else
                    {
                        await Tools.ReplyAsync(e, $"Error: {ex.Message}");
                        return;
                    }
                }
        };

        public static Func<CommandArgs, Task> UrbanDictionary = async e =>
        {
            using (HttpClient client = new HttpClient())
            {
                string response = await client.GetStringAsync($"http://api.urbandictionary.com/v0/define?term={e.ArgText}");
                dynamic json = JsonConvert.DeserializeObject(response);

                if (json.result_type.ToString() == "no_results")
                {
                    await Tools.ReplyAsync(e, $"Could not find the definition of {e.ArgText}");
                    return;
                }

                string message = $"\nDefinition of {e.ArgText}:\n```{json.list[0].definition.ToString()}```\n\nExample:\n```{json.list[0].example.ToString()}```\n";
                message += $"Permalink: <http://www.urbandictionary.com/define.php?term={WebUtility.UrlEncode(e.ArgText)}>";
                await Tools.ReplyAsync(e, message);
            }
        };
    }
}
