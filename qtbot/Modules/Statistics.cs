using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace qtbot.Modules
{
    public static class Statistics
    {
        static DateTime todayMessage;
        static Dictionary<ulong, int> messageCount = new Dictionary<ulong, int>();

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

            }
        }

        public static async Task CheckNewDay()
        {
            if((DateTime.Now - todayMessage).TotalDays > 1)
            {
                todayMessage = DateTime.Now;

                //Save it to the file.
                FileStream messageFile = File.Open("LocalFiles/regulars.txt", FileMode.Append);
                double average = 0;

                foreach(var message in messageCount)
                {
                    average += message.Value;
                }

                average /= messageCount.Count;

                int passedDays = 0;
                

                using (var ws = new StreamWriter(messageFile))
                {
                    await ws.WriteLineAsync($"[{passedDays}]\n{average}");
                }
            }
        }
    }
}
