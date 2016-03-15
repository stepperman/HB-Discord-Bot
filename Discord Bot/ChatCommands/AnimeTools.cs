using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord_Bot.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using System.Xml.Linq;

namespace Discord_Bot
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

                string lifeAnime = Tools.CalculateTime(life_spent_on_anime);

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

                await Tools.Reply(e, messageToPost);

            }
            catch (Exception ex)
            {
                await Tools.Reply(e, $"Error: {ex.Message}");
            }
        };

        public static Func<CommandArgs, Task> GetAnimeFromMAL = async e =>
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    var mallogin = System.Text.Encoding.UTF8.GetBytes(System.Convert.ToString(Program.ProgramInfo.mallogin));
                    string base64 = System.Convert.ToBase64String(mallogin);
                    client.Headers.Add("Authorization", $"Basic {base64}");
                    client.QueryString.Add("q", Uri.EscapeUriString(e.ArgText));
                    string response = await client.DownloadStringTaskAsync("http://myanimelist.net/api/anime/search.xml");

                    var xml = XDocument.Parse(response);
                    var anime = xml.Element("anime").Descendants("entry").FirstOrDefault();

                    string title = anime.Element("title").Value;
                    string episodes = anime.Element("episodes").Value;
                    string type = anime.Element("type").Value;
                    string id = anime.Element("id").Value;
                    string synopsis = anime.Element("synopsis").Value;
                    string score = anime.Element("score").Value;

                    ///TITLE NAME (type) | Episodes: episodes | Mallink | Score: 
                    ///synopsis
                    string reply = $"**{title}** ({type}) | **Episodes:** {episodes} | http://myanimelist.net/anime/{id} | **Score** {score}/10.0 {Environment.NewLine}{synopsis}";
                    reply = reply.Replace("<br />", "");

                    await Tools.Reply(e, reply, false);
                }
            }
            catch (Exception ex)
            {
                await Tools.Reply(e, $"Error: {ex.Message}");
            }
        };

        public static Func<CommandArgs, Task> GetMangaFromMAL = async e =>
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    var mallogin = System.Text.Encoding.UTF8.GetBytes(System.Convert.ToString(Program.ProgramInfo.mallogin));
                    string base64 = System.Convert.ToBase64String(mallogin);
                    client.Headers.Add("Authorization", $"Basic {base64}");
                    client.QueryString.Add("q", Uri.EscapeUriString(e.ArgText));
                    string response = await client.DownloadStringTaskAsync("http://myanimelist.net/api/manga/search.xml");

                    var xml = XDocument.Parse(response);

                    XElement anime = null;
                    foreach (var entry in xml.Element("manga").Descendants("entry"))
                    {
                        if (entry.Element("type").Value.ToLower() == "manga")
                        {
                            anime = entry;
                            break;
                        }
                    }

                    if (anime == null)
                    {
                        await Tools.Reply(e, "Manga not found.");
                        return;
                    }

                    string title = anime.Element("title").Value;
                    string chapters = anime.Element("chapters").Value;
                    string volumes = anime.Element("volumes").Value;
                    string id = anime.Element("id").Value;
                    string synopsis = anime.Element("synopsis").Value;
                    string score = anime.Element("score").Value;
                    string status = anime.Element("status").Value;

                    ///TITLE NAME (Manga) | status |  Chapters: episodes Volumes: volumes | Score: 
                    ///
                    ///synopsis

                    string reply = "";

                    if (status == "Finished")
                        reply = $"**{title}** (Manga) | **Status:** {status} **Chapters:** {chapters} **Volumes:** {volumes} | **Score** {score}/10.0 | {Environment.NewLine}http://myanimelist.net/manga/{id} |  {Environment.NewLine}{synopsis}";
                    else
                        reply = $"**{title}** (Manga) | **Status:** {status} | **Score** {score}/10.0 | {Environment.NewLine}http://myanimelist.net/manga/{id} |  {Environment.NewLine}{synopsis}";
                    reply = reply.Replace("<br />", "");

                    await Tools.Reply(e, reply, false);
                }
            }
            catch (Exception ex)
            {
                await Tools.Reply(e, $"Error: {ex.Message}");
            }
        };
    }
}
