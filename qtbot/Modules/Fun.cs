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
using qtbot.CommandPlugin.Attributes;
using System.Drawing;

namespace qtbot.Modules
{
    static class Fun
    {
        [Command("8ball"), Description("The magic 8-ball will answer all your doubts and questions! It's not rigged, I swear.")]
        public static async Task MagicEightBall(CommandArgs e)
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
        }

        [Command("ayy"), Description("ayy it."), Cooldown(2, Cooldowns.Minutes)]
        public static async Task Ayy(CommandArgs e)
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
        }

        [Command("img"), Cooldown(1, Cooldowns.Minutes)]
        [Description("Get an image from the shitty Google Custom Search engine that will be replaced soon.")]
        public static async Task ImageFromGoogle(CommandArgs e)
        {
            QtNetHelper.QtNet qtNet = new QtNetHelper.QtNet("https://www.googleapis.com/customsearch/v1");

                qtNet.Query = new Dictionary<string, string>
                {
                    { "key", Uri.EscapeUriString((string)Storage.programInfo.google_key_code) },
                    { "cx", Uri.EscapeUriString((string)Storage.programInfo.google_cx_code) },
                    { "searchType", "image" },
                    { "q", Uri.EscapeUriString(e.ArgText) },
                    { "safe", Tools.GetServerInfo(e.Guild.Id).safesearch },
                    { "num", "10" }
                };
                
                try
                {
                    dynamic json = JsonConvert.DeserializeObject(await qtNet.GetStringAsync());
                    if(json.items.Count == 0) { await Tools.ReplyAsync(e, "No images have been found :("); return; }

                    string link = json.items[Tools.random.Next(0, Enumerable.Count(json.items))].link;
                    await Tools.ReplyAsync(e, link);
                }
                catch (Exception ex)
                {
                    if ((ex as WebException)?.Status == WebExceptionStatus.ProtocolError)
                    {
                        await Tools.ReplyAsync(e, "The daily limit has been reached. Try again tomorrow!");
                        return;
                    }
                    else if ((ex as Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) != null)
                    {
                        await Tools.ReplyAsync(e, "I couldn't find any images :(. Don't blame me, blame Google.");
                        return;
                    }
                    else
                    {
                        await Tools.ReplyAsync(e, $"Error: {ex.Message}");
                        return;
                    }
                }
        }

        [Command("ud"), Description("Find the definition of a word with Urban Dictionary")]
        public static async Task UrbanDictionary(CommandArgs e)
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
        }

        [Command("fortune"),
            Description("Grab a fortune cookie and see the message.")]
        public static async Task FortuneCookie(CommandArgs e)
        {
            var tpyingState = e.Channel.EnterTypingState();

            string[] responses;
            //Get an array of possible responses
            using (StreamReader sr = new StreamReader(File.OpenRead("LocalFiles/fortunecookie.txt")))
            {
                responses = (await sr.ReadToEndAsync()).Split('\n');
            }

            Bitmap bitmap = new Bitmap("LocalFiles/fortunecookie.png");
            RectangleF rect = new RectangleF(247, 133, 528 - 247, 210 - 133);
            Graphics g = Graphics.FromImage(bitmap);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            g.DrawString(responses[Tools.random.Next(responses.Length)],
                new Font("Calibri", 20f), Brushes.Black, rect, stringFormat);

            g.Flush();

            var memStream = new MemoryStream();
            bitmap.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
            memStream.Position = 0;

            await e.Channel.SendFileAsync(memStream, "fortune.png", e.Author.Mention);

            tpyingState.Dispose();
        }
    }
}
