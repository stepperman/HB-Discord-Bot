using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Discord_Bot.CommandPlugin;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Xml.Linq;
using MALAPI;

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
                MyAnimeListAPI api = new MyAnimeListAPI();
                var links = await api.GetSearchResultLinks(e.ArgText, 1);
                
                var anime = await api.GetAnimeMalLink(links[0]);

                string epis = anime.Episodes == 0 ? "Unkown" : anime.Episodes.ToString();
                string reply = $"**{anime.Title}** ({anime.Type}) \n {links[0]}\n**Score**: {anime.Score}" +
                $"\n**Episodes:** {epis}\n**Genres:** {String.Join(", ", anime.Genres)}\n\n{anime.Synopsis}";

                await Tools.Reply(e, reply, false);
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
                    var mallogin = System.Text.Encoding.UTF8.GetBytes(System.Convert.ToString(Storage.programInfo.mallogin));
                    string base64 = System.Convert.ToBase64String(mallogin);
                    client.Headers.Add("Authorization", $"Basic {base64}");
                    client.QueryString.Add("q", Uri.EscapeUriString(e.ArgText));
                    string response = await client.DownloadStringTaskAsync("https://myanimelist.net/api/manga/search.xml");

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
                        reply = $"**{title}** (Manga) http://myanimelist.net/manga/{id} \n**Status:** {status} \n**Chapters:** {chapters} \n**Volumes:** {volumes} \n\n**Score** {score}/10.0 \n{synopsis}";
                    else
                        reply = $"**{title}** (Manga) https://myanimelist.net/manga/{id} \n**Status:** {status} \n**Score** {score}/10.0 \n\n{synopsis}";
                    reply = reply.Replace("<br />", "");

                    await Tools.Reply(e, reply, false);
                }
            }
            catch (Exception ex)
            {
                await Tools.Reply(e, $"Error: {ex.Message}");
            }
        };

        public static Func<CommandArgs, Task> AnimeFromAnilist = async e =>
        {
            //Check if we need a new authorization token
            if ((DateTime.Now - Storage.anilistAuthorizationCreated).TotalMinutes > 50)
            {
                if (!await AuthorizeAnilist())
                {
                    await Tools.Reply(e, "Something went wrong authorizing Anilist, please try again?");
                    return;
                }
            }

            string url = "https://anilist.co/api/anime/search/";
            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

            using (WebClient wc = new WebClient())
            {
                wc.QueryString = new NameValueCollection
                {
                    { "access_token", (string)Storage.anilistAccessToken },
                };

                var response = await wc.DownloadStringTaskAsync(url + e.ArgText);
                dynamic json = JsonConvert.DeserializeObject(response);
                dynamic anime = json[0];

                //Download image
                byte[] image = await wc.DownloadDataTaskAsync((string)anime.image_url_lge);
                Stream stream = new MemoryStream(image);

                string episodes = anime.total_episodes == 0 ? "unknown" : (string)anime.total_episodes;
                string duration = anime.duration == null ? "" : $"\n**Duration:** {(int)anime.duration} minutes";

                var reply = $@"
**Anime:** {(string)anime.title_english}
**Score:** {(string)anime.average_score}/100
**Episodes:** {episodes} {duration}
**Type:** {anime.type}
**Genres:** {String.Join(", ", anime.genres)}
**Description:**
{((string)anime.description).Replace("<br>", "")}
https://anilist.co/anime/{(string)anime.id}";

                await e.Channel.SendMessage(reply);
                await e.Channel.SendFile("coolimage.jpg", stream);
            }
        };

        public static async Task<bool> AuthorizeAnilist()
        {
            

            string url = "https://anilist.co/api/auth/access_token";
            var values = new NameValueCollection
            {
                { "grant_type", "client_credentials" },
                { "client_id", (string)Storage.programInfo.anilist_id },
                { "client_secret", (string)Storage.programInfo.anilist_client_secret }
            };

            try
            {
                using (WebClient wc = new WebClient())
                {
                    ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
                    var response = await wc.UploadValuesTaskAsync(url, values);
                    string json = System.Text.Encoding.UTF8.GetString(response);
                    dynamic parsedJson = JsonConvert.DeserializeObject(json);

                    Storage.anilistAccessToken = (string)parsedJson.access_token;
                    Storage.anilistAuthorizationCreated = DateTime.Now;

                    return true;
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return false; }
        }
    }
}
