using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using qtbot.CommandPlugin.Attributes;
using qtbot.CommandPlugin;

namespace qtbot.Modules
{
    public static class Statistics
    {
        static DateTime todayMessage = DateTime.Now;
        static Dictionary<ulong, int> messageCount = new Dictionary<ulong, int>();
        private static ulong[] ignoreChannels =
        {
            255347719705591809,
            241365822671552512,
            134267667245694976
        };

        public static async Task ReceiveMessage(IMessage message)
        {
            if (todayMessage == null)
                todayMessage = DateTime.Now;

            await CheckNewDay();

            var messageChannel = message.Channel as IGuildChannel;
            if (messageChannel == null)
                return;

            if(messageChannel.Guild.Id == 99333280020566016)
            {
                //Skip #ayy, #admin-chat and #peasantry (or any channels in the array)
                if (ignoreChannels.Any(x => x == messageChannel.Id))
                    return;

                //Increment the message by 1 if the Author is already in the list, if not, create it. Start with 1 because a message has obviously been sent.
                if (messageCount.ContainsKey(message.Author.Id))
                    messageCount[message.Author.Id]++;
                else
                    messageCount.Add(message.Author.Id, 1);
            }
        }

        [Command("amsg")]
        public static async Task GetAverageOfAll(CommandArgs e)
        {
            var file = File.Open("LocalFiles/regulars.txt", FileMode.OpenOrCreate, FileAccess.Read);
            using (StreamReader reader = new StreamReader(file))
            {
                var text = await reader.ReadToEndAsync();
                if (text.Length != 0)
                {
                    var splitString = GetSplittedString(text);
                    double average = 0;
                    for(int i = 1; i < splitString.Length; i = i+2) //This just makes me angry.
                    {
                        average += double.Parse(splitString[i]);
                    }
                    average /= Math.Round((double)(splitString.Length / 2), 2);

                    await BotTools.Tools.ReplyAsync(e, $"The average amout of messages in {(int)(splitString.Length / 2)} days is {average}");
                    return;
                }
                await BotTools.Tools.ReplyAsync(e, "Not enough messages have been sent yet to caluclate the average.");

            }

            file.Dispose();
        }

        public static async Task CheckNewDay()
        {
            if((DateTime.Now - todayMessage).TotalDays > 1.0d)
            {
                todayMessage = DateTime.Now;

                //Get passed days
                int passedDays = 0;

                var file = File.Open("LocalFiles/regulars.txt", FileMode.OpenOrCreate, FileAccess.Read);
                using (StreamReader reader = new StreamReader(file))
                {
                    var text = await reader.ReadToEndAsync();
                    if(text.Length != 0)
                    {
                        var splitString = GetSplittedString(text);
                        string strPassedDays = splitString[splitString.Length - 2];
                        //Remove the first and the last character.
                        strPassedDays = strPassedDays.Substring(1).Remove(strPassedDays.Length - 2);
                        int.TryParse(strPassedDays, out passedDays);
                    }
                }

                //Save it to the file.
                FileStream messageFile = File.Open("LocalFiles/regulars.txt", FileMode.Append);
                double average = 0;

                foreach(var message in messageCount)
                {
                    average += message.Value;
                }

                average /= messageCount.Count;
                
                using (var ws = new StreamWriter(messageFile))
                {
                    await ws.WriteLineAsync($"[{passedDays}]\n{average}");
                }

                //Clear the list
                messageCount.Clear();
                //Dispose of the streams.
                file.Dispose();
                messageFile.Dispose();
            }
        }

        private static string[] GetSplittedString(string split)
        {
            string regex = @"(\[.*?\])";

            var z = Regex.Split(split, regex);
            for (int i = 0; i < z.Length; i++)
            {
                z[i] = Regex.Replace(z[i], @"(\\n|\\r)", "");
            }
            //Remove the first entry in the array because for some reason it's empty.
            var x = z.ToList();
            x.RemoveAt(0);
            return x.ToArray();
        }
    }
}
