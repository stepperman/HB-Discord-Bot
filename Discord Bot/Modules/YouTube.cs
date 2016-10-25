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
        private static int waitingTime = 1; //Waits one minute until invalidating the selector.
        private static List<YouTubeSelector> utubeSelectors = new List<YouTubeSelector>();

        /// <summary>
        /// Find a YouTube video.
        /// </summary>
        public static Func<CommandArgs, Task> FindYouTubeVideo = async e =>
        {
            if (WaitingOnUser(e.User.Id))
                return;

            //Should probably not publically expose the key, huh?
            string url = "https://www.googleapis.com/youtube/v3/search";

            using (var client = new WebClient())
            {
                client.QueryString = new System.Collections.Specialized.NameValueCollection
                {
                    { "part", $"snippet" },
                    { "q" , $"{e.ArgText}" },
                    { "maxResults", "5" },
                    { "key", "AIzaSyAnXBGxB_QWlHzfn0PGF7oYVdDnKwjQW4I" }
                };

                try
                {
                    var jsonString = await client.DownloadStringTaskAsync(url);
                    dynamic json = JsonConvert.DeserializeObject(jsonString);

                    string[] urlIds = new string[5];
                    string[] titles = new string[5];

                    for (int i = 0; i < urlIds.Length; i++)
                    {
                        urlIds[i] = Convert.ToString(json.items[i].id.videoId);
                        titles[i] = Convert.ToString(json.items[i].snippet.title);

                        if (urlIds[i] == null || urlIds[i] == "")
                            break;
                    }

                    var selector = YouTubeSelector.NewSelector(e, urlIds);

                    string messageToSend = "I've found a couple videos you can choose from! Just write a message containing the number!\n";

                    for (int i = 0; i < titles.Length; i++)
                    {
                        if (titles[i] == null || titles[i] == "")
                            break;

                        messageToSend += (i + 1) + " - " + titles[i] + "\n";
                    }

                    //Queue the message to be deleted once answered.
                    selector.messageToDelete.Add(await Tools.Reply(e, messageToSend));

                }
                catch (Exception)
                {
                    await Tools.Reply(e, "I couldn't find anything!");
                }
            }
        };

        /// <summary>
        /// Check to see if already waiting on the user to send a message..
        /// </summary>
        /// <param name="userID">the user's id to check</param>
        /// <returns>a fucking boolean</returns>
        private static bool WaitingOnUser(ulong userID)
        {
            if (FindUser(userID) == null)
                return false;

            return true;
        }

        private static YouTubeSelector FindUser(ulong userID)
        {
            var set = new HashSet<YouTubeSelector>(utubeSelectors);
            return set.FirstOrDefault(x => x.cmdArgs.User.Id == userID);
            
        }

        public static async Task ReceivedMessage(Message message)
        {
            var a = FindUser(message.User.Id);

            if (a == null)
                return;

            a.messageToDelete.Add(message);
            
            //Update the list so it's cleared.
            UpdateList();

            int number = 0;
            if (!int.TryParse(message.Text, out number) || number < 1 || number > 5)
                return;

            var urlid = a.urlIds[number-1];
            if (urlid == null|| urlid == "")
                return;

            await Tools.Reply(message.User, message.Channel, $"https://youtube.com/watch?v={urlid}", true);

            try
            {
                //Delete all the messages
                foreach (var msg in a.messageToDelete)
                {
                    await msg.Delete();
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Clear the list for users that haven't messaged longer than the waiting time.
        /// </summary>
        private static void UpdateList()
        {
            utubeSelectors.RemoveAll(x => (DateTime.Now - x.startedWaiting).TotalMinutes >= waitingTime);
        }

        class YouTubeSelector
        {
            public CommandArgs cmdArgs { get; private set; }
            public string[] urlIds { get; private set; }
            public DateTime startedWaiting { get; private set; }
            public List<Message> messageToDelete = new List<Message>();

            /// <summary>
            /// Create a new selector and add it to the list
            /// </summary>
            /// <param name="cmdArgs">the channel and user it's waiting for a message.</param>
            /// <param name="urlIds">all the YouTube url Ids</param>
            public YouTubeSelector(CommandArgs cmdArgs, string[] urlIds)
            {
                this.cmdArgs = cmdArgs;
                this.urlIds = urlIds;
                startedWaiting = DateTime.Now;

                //Add it to the list
                utubeSelectors.Add(this);
            }

            public static YouTubeSelector NewSelector(CommandArgs cmdArgs, string[] urlIds)
            {
                return new YouTubeSelector(cmdArgs, urlIds);
            }
        }
    }
}
