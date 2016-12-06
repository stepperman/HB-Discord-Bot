﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using qtbot.CommandPlugin;
using qtbot.BotTools;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Xml.Linq;
using QtNetHelper;
using Discord;

namespace qtbot.Modules
{
    class AnimeTools
    {
        public static Func<CommandArgs, Task> GetHBUser = async e =>
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

                string lifeAnime = BotTools.Tools.CalculateTime(life_spent_on_anime);

                string messageToPost = $@"
**User**: {username}
**Avatar**: {avatar} 
**{waifu_prefix}**: {waifu}
**Bio:** {bio}
**Time wasted on Anime:** {lifeAnime}";

                if (!String.IsNullOrWhiteSpace(location))
                    messageToPost += $"\n**Location:** {location}";
                if (!String.IsNullOrWhiteSpace(website))
                    messageToPost += $"\n**Website:** {website}";

                messageToPost += $"\n{userUrl}";

                await BotTools.Tools.ReplyAsync(e, messageToPost);

            }
            catch (Exception ex)
            {
                await BotTools.Tools.ReplyAsync(e, $"Error: {ex.Message}");
            }
        };

        public static Func<CommandArgs, Task> AnimeFromAnilist = async e =>
        {
            //Check if we need a new authorization token
            if ((DateTime.Now - Storage.anilistAuthorizationCreated).TotalMinutes > 50)
            {
                if (!await AuthorizeAnilistAsync())
                {
                    await Tools.ReplyAsync(e, "Something went wrong authorizing Anilist, please try again?");
                    return;
                }
            }

            try
            {

                string url = $"https://anilist.co/api/anime/search/{e.ArgText}";

                QtNet wc = new QtNet(url);
                wc.Query.Add("access_token", Storage.anilistAccessToken);

                var response = await wc.GetStringAsync();
                dynamic json = JsonConvert.DeserializeObject(response);
                dynamic anime = json[0];

                //Download image
                wc.BaseUrl = (string)anime.image_url_lge;

                string episodes = anime.total_episodes == 0 ? "unknown" : (string)anime.total_episodes;
                string duration = String.IsNullOrWhiteSpace(Convert.ToString(anime.duration)) ? "" : $"{(int)anime.duration} minutes";

                string description = ((string)anime.description).Replace("<br>", "");

                if(description.Length >= 1024)
                    description = description.Remove(1024 - 5) + "...";

                EmbedBuilder eb = new EmbedBuilder();
                eb.WithColor(new Color(0, 255, 0))
                .WithTitle((string)anime.title_english)
                .WithDescription(String.Join(", ", anime.genres))
                .WithUrl($"https://anilist.co/anime/{(string)anime.id}")
                .WithImageUrl(wc.BaseUrl);

                //Add Episodes field
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Episodes";
                    x.Value = episodes;
                });

                if (!String.IsNullOrEmpty(duration))
                {
                    eb.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Duration";
                        x.Value = duration;
                    });
                }

                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Score";
                    x.Value = $"{(string)anime.average_score}/100";
                });

                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Type";
                    x.Value = (string)anime.type;
                });

                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Description";
                    x.Value = description;
                });

                await e.Channel.SendMessageAsync("", embed: eb);
            }
            catch(Exception ex)
            {
                if (ex is ArgumentException)
                    await Tools.ReplyAsync(e, "That anime does not exist.");
                else
                    await Tools.ReplyAsync(e, ex.Message);
            }
        };

        public static async Task<bool> AuthorizeAnilistAsync()
        {
            string url = "https://anilist.co/api/auth/access_token";
            
            try
            {
                QtNet qtNet = new QtNet(url);
                qtNet.Query = new Dictionary<string, string>
                    {
                        { "grant_type", "client_credentials" },
                        { "client_id", (string)Storage.programInfo.anilist_id },
                        { "client_secret", (string)Storage.programInfo.anilist_client_secret }
                    };

                    var response = await qtNet.PostAsync();
                    string json = await response.Content.ReadAsStringAsync();
                    dynamic parsedJson = JsonConvert.DeserializeObject(json);

                    Storage.anilistAccessToken = (string)parsedJson.access_token;
                    Storage.anilistAuthorizationCreated = DateTime.Now;

                    return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}