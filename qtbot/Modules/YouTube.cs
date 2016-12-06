using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using qtbot.CommandPlugin;
using qtbot.BotTools;
using System.Net;
using Newtonsoft.Json;
using Discord.WebSocket;

namespace qtbot.Modules
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
            if (WaitingOnUser(e.AuthorId))
                return;

            //Should probably not publically expose the key, huh?
            string url = "https://www.googleapis.com/youtube/v3/search";

            QtNetHelper.QtNet qtNet = new QtNetHelper.QtNet(url)
            {
                Query = new Dictionary<string, string>
                {
                    { "part", $"snippet" },
                    { "q" , $"{e.ArgText}" },
                    { "maxResults", "5" },
                    { "type", "video" },
                    { "key", "AIzaSyAnXBGxB_QWlHzfn0PGF7oYVdDnKwjQW4I" }
                }
            };

            try
                {
                    var jsonString = await qtNet.GetStringAsync();
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
                    selector.messageToDelete.Add(await Tools.ReplyAsync(e, messageToSend));

                }
                catch (Exception)
                {
                    await Tools.ReplyAsync(e, "I couldn't find anything!");
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
            return set.FirstOrDefault(x => x.CmdArgs.AuthorId == userID);
            
        }

        public static async Task ReceivedMessageAsync(IMessage message)
        {
            var a = FindUser(message.Author.Id);

            if (a == null)
                return;

            
            
            //Update the list so it's cleared.
            UpdateList();

            int number = 0;
            if (!int.TryParse(message.Content, out number) || number < 1 || number > 5)
                return;

            a.messageToDelete.Add(message);

            var urlid = a.URLIds[number-1];
            if (urlid == null|| urlid == "")
                return;

            await Tools.ReplyAsync(message.Author as SocketUser, message.Channel, $"https://youtube.com/watch?v={urlid}", true);

            utubeSelectors.Remove(a);

            try
            {
                //Delete all the messages
                foreach (var msg in a.messageToDelete)
                {
                    await msg.DeleteAsync();
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Clear the list for users that haven't messaged longer than the waiting time.
        /// </summary>
        private static void UpdateList()
        {
            utubeSelectors.RemoveAll(x => (DateTime.Now - x.StartedWaiting).TotalMinutes >= waitingTime);
        }

        class YouTubeSelector
        {
            public CommandArgs CmdArgs { get; private set; }
            public string[] URLIds { get; private set; }
            public DateTime StartedWaiting { get; private set; }
            public List<IMessage> messageToDelete = new List<IMessage>();

            /// <summary>
            /// Create a new selector and add it to the list
            /// </summary>
            /// <param name="cmdArgs">the channel and user it's waiting for a message.</param>
            /// <param name="urlIds">all the YouTube url Ids</param>
            public YouTubeSelector(CommandArgs cmdArgs, string[] urlIds)
            {
                this.CmdArgs = cmdArgs;
                this.URLIds = urlIds;
                StartedWaiting = DateTime.Now;

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
