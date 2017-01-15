using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using qtbot.CommandPlugin;
using qtbot.BotTools;
using qtbot.CommandPlugin.Attributes;
using Newtonsoft.Json.Linq;
using QtNetHelper;
using Discord;

namespace qtbot.Modules
{
    class AnimeTools
    {
        private static async Task<bool> IsAnimeListAuthorized(ITextChannel e)
        {
            if ((DateTime.Now - Storage.anilistAuthorizationCreated).TotalMinutes > 50)
            {
                if (!await AuthorizeAnilistAsync())
                {
                    await e.SendMessageAsync($"Something went wrong authorizing Anilist, please try again!");
                    return false;
                }
            }
            return true;
        }

        [Command("al"), Description("Get a user from Anilist.")]
        public static async Task UserFromAnilist(CommandArgs e)
        {
            if (!await IsAnimeListAuthorized(e.Channel))
                return;

            QtNet qtNet = new QtNet("https://anilist.co/api/user/" + e.ArgText);
            qtNet.AddQuery("access_token", Storage.anilistAccessToken);
            dynamic response = JsonConvert.DeserializeObject(await qtNet.GetStringAsync());


            string description = ((string)response.about).Replace("<br>", "");
            if (description.Length >= 1024)
            {
                description.Substring(0, 1024 - 5);
                description += "...";
            }
            EmbedBuilder embed = new EmbedBuilder()
            .WithTitle((string)response.display_name)
            .WithDescription(description)
            .WithThumbnailUrl((string)response.image_url_lge)
            .WithColor(new Color(255, 0, 0))
            .WithUrl($"http://anilist.co/user/{(string)response.display_name}")
            .AddField(x =>
            {
                x.Name = "Anime time";
                x.Value = Tools.CalculateTime((Int32)response.anime_time);
                x.IsInline = false;
            })
            .AddField(x => x.WithName("Manga chapters").WithIsInline(false).WithValue((string)response.manga_chap));

            await e.Channel.SendMessageAsync("", embed: embed);
        }

        [Command("ku"), Description("Get a user from Kitsu.")]
        public static async Task UserFromKitsu(CommandArgs e)
        {
            QtNet qtNet = new QtNet("https://kitsu.io/api/edge/users");
            qtNet.AddQuery("filter[name]", e.ArgText);

            dynamic response = JsonConvert.DeserializeObject(await qtNet.GetStringAsync());

            if(response.data.Count == 0)
            {
                await Tools.ReplyAsync(e, "Could not find user.");
                return;
            }

            var user = response.data[0];
            string name = user.attributes.name;
            string description = user.attributes.about;
            string timeSpentOnAnime = Tools.CalculateTime((int)user.attributes.lifeSpentOnAnime);

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(name)
                .WithDescription(description)
                .WithThumbnailUrl((string)user.attributes.avatar.medium)
                .WithColor(new Color(0xEC9614))
                .WithUrl("https://kitsu.io/users/" + name)
                .AddField(x =>
                {
                    x.Name = "Anime time";
                    x.Value = timeSpentOnAnime;
                    x.IsInline = false;
                });

            await e.Channel.SendMessageAsync("", embed: embed);
        }

        [Command("anime", CommandType.User),  
            Description("Get an anime from anilist")]
        public static async Task AnimeFromAL(CommandArgs e)
        {
            //Check if we need a new authorization token
            if (!await IsAnimeListAuthorized(e.Channel))
                return;

            try
            {

                string url = $"https://anilist.co/api/anime/search/{e.ArgText}";

                QtNet wc = new QtNet(url);
                wc.Query.Add("access_token", Storage.anilistAccessToken);

                var response = await wc.GetStringAsync();
                dynamic json = JsonConvert.DeserializeObject(response);

                List<Models.AnimeModel> l = new List<Models.AnimeModel>();
                for (int i = 0; i < 10; i++)
                {
                    if (i == json.Count)
                        break;
                    dynamic anime = json[i];


                    string episodes = anime.total_episodes == 0 ? "unknown" : (string)anime.total_episodes;
                    string duration = String.IsNullOrWhiteSpace(Convert.ToString(anime.duration)) ? "" : $"{(int)anime.duration} minutes";
                    string description = ((string)anime.description).Replace("<br>", "");

                    if (description.Length >= 1024)
                        description = description.Remove(1024 - 5) + "...";

                    l.Add(new Models.AnimeModel()
                    {
                        Title = (string)anime.title_english,
                        Genre = String.Join(", ", anime.genres),
                        Url = $"https://anilist.co/anime/{(string)anime.id}",
                        ImageUrl = (string)anime.image_url_lge,
                        Episodes = episodes,
                        Duration = duration,
                        Description = description,
                        Score = $"{(string)anime.average_score}/100",
                        Type = (string)anime.type
                    });
                }

                if (l.Count == 1)
                {
                    await MakeAnimeObjectAsync(e.Message, e.Author as IGuildUser, l[0]);
                    return;
                }

                var selector = await MultipleSelector.MultiSelectorController.CreateSelectorAsync<Models.AnimeModel>
                    (e.Message, l.ToArray());
                selector.AddDeleteMessage(e.Message);

                selector.SetResponse(async (a, b, c) => await MakeAnimeObjectAsync(a, b, c));
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                    await Tools.ReplyAsync(e, "That anime does not exist.");
                else
                    await Tools.ReplyAsync(e, ex.Message);
            }
        }

        public static async Task MakeAnimeObjectAsync(IMessage message, IGuildUser author, object obj)
        {
            var anime = obj as Models.AnimeModel;
            if (anime == null)
            {
                await message.Channel.SendMessageAsync($"{author.Mention}: Something went wrong! I'm so sorry :(");
                return;
            }

            EmbedBuilder eb = new EmbedBuilder();
            eb.WithColor(new Color(0, 255, 0))
                .WithTitle(anime.Title)
                .WithDescription(anime.Genre)
                .WithUrl(anime.Url)
                .WithThumbnailUrl(anime.ImageUrl)
                .WithAuthor(x =>
                {
                    x.Name = author.Nickname == null ? author.Username : author.Nickname;
                    x.IconUrl = author.AvatarUrl;
                });

            //Add Episodes field
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Episodes";
                x.Value = anime.Episodes;
            });

            if (!String.IsNullOrEmpty(anime.Duration))
            {
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Duration";
                    x.Value = anime.Duration;
                });
            }

            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Score";
                x.Value = anime.Score;
            });

            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Type";
                x.Value = anime.Type;
            });

            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Description";
                x.Value = anime.Description;
            });

            await message.Channel.SendMessageAsync("", embed: eb);
        }

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
