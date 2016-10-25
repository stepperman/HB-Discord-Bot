using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord_Bot.CommandPlugin;
using System.Net;
using Newtonsoft.Json;

namespace Discord_Bot.Modules
{
    class YouTube
    {
        /// <summary>
        /// Find a YouTube video.
        /// </summary>
        public static Func<CommandArgs, Task> FindYouTubeVideo = async e =>
        {
            //Should probably not publically expose the key, huh?
            string url = "https://www.googleapis.com/youtube/v3/search";

            using (var client = new WebClient())
            {
                client.QueryString = new System.Collections.Specialized.NameValueCollection
                {
                    { "part", $"snippet" },
                    { "q" , $"{e.ArgText}" },
                    { "maxResults", "1" },
                    { "key", "AIzaSyAnXBGxB_QWlHzfn0PGF7oYVdDnKwjQW4I" }
                };

                try
                {
                    var jsonString = await client.DownloadStringTaskAsync(url);
                    dynamic json = JsonConvert.DeserializeObject(jsonString);

                    string uTubeID = Convert.ToString(json.items[0].id.videoID);
                    await Tools.Reply(e, $"https://www.youtube.com/watch?v={uTubeID}");
                }
                catch (Exception ex)
                {
                    await Tools.Reply(e, "I couldn't find anything!");
                }
            }
        };
    }
}
