using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;
using qtbot.CommandPlugin;
using Newtonsoft.Json;
using System.Reflection;

namespace qtbot.BotTools
{
    static class Tools
    {
        
        public static Random random = new Random();
        
        static Tools()
        {
            if (File.Exists("./LocalFiles/ServerInfo.json"))
            {
                var sw = new StreamReader(File.Open("./LocalFiles/ServerInfo.json", FileMode.Open));

                string json = sw.ReadToEnd();
                Storage.serverInfo = JsonConvert.DeserializeObject<Dictionary<ulong, ServerInfo>>(json);
                sw.Dispose();
            }
        }
        
        public static string CalculateTime(int minutes)
        {
            if (minutes == 0)
                return "No time.";

            int years, months, days, hours = 0;

            hours = minutes / 60;
            minutes %= 60;
            days = hours / 24;
            hours %= 24;
            months = days / 30;
            days %= 30;
            years = months / 12;
            months %= 12;

            string animeWatched = "";

            if (years > 0)
            {
                animeWatched += years;
                if (years == 1)
                    animeWatched += " **year**";
                else
                    animeWatched += " **years**";
            }

            if (months > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += ", ";
                animeWatched += months;
                if (months == 1)
                    animeWatched += " **month**";
                else
                    animeWatched += " **months**";
            }

            if (days > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += ", ";
                animeWatched += days;
                if (days == 1)
                    animeWatched += " **day**";
                else
                    animeWatched += " **days**";
            }

            if (hours > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += ", ";
                animeWatched += hours;
                if (hours == 1)
                    animeWatched += " **hour**";
                else
                    animeWatched += " **hours**";
            }

            if (minutes > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += " and ";
                animeWatched += minutes;
                if (minutes == 1)
                    animeWatched += " **minute**";
                else
                    animeWatched += " **minutes**";
            }

            return animeWatched;
        }

        public static string CalculateTimeWithSeconds(int seconds)
        {
            if (seconds == 0)
                return "No time.";

            int years, minutes, months, days, hours = 0;

            minutes = seconds / 60;
            seconds %= 60;
            hours = minutes / 60;
            minutes %= 60;
            days = hours / 24;
            hours %= 24;
            months = days / 30;
            days %= 30;
            years = months / 12;
            months %= 12;

            string animeWatched = "";

            if (years > 0)
            {
                animeWatched += years;
                if (years == 1)
                    animeWatched += " **year**";
                else
                    animeWatched += " **years**";
            }

            if (months > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += ", ";
                animeWatched += months;
                if (months == 1)
                    animeWatched += " **month**";
                else
                    animeWatched += " **months**";
            }

            if (days > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += ", ";
                animeWatched += days;
                if (days == 1)
                    animeWatched += " **day**";
                else
                    animeWatched += " **days**";
            }

            if (hours > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += ", ";
                animeWatched += hours;
                if (hours == 1)
                    animeWatched += " **hour**";
                else
                    animeWatched += " **hours**";
            }

            if (minutes > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += ",";
                animeWatched += minutes;
                if (minutes == 1)
                    animeWatched += " **minute**";
                else
                    animeWatched += " **minutes**";
            }

            if (seconds > 0)
            {
                if (animeWatched.Length > 0)
                    animeWatched += " and ";
                animeWatched += seconds;
                if (seconds == 1)
                    animeWatched += " **second**";
                else
                    animeWatched += " **seconds**";
            }

            return animeWatched;
        }

        #region messaging
        public static async Task<IUserMessage> ReplyAsync(SocketUser user, IMessageChannel channel, string text, bool mentionUser)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (channel as IPrivateChannel != null || !mentionUser)
                        return await channel.SendMessageAsync(text);
                    else
                        return await channel.SendMessageAsync($"{user.Mention}: {text}");
                }
            }
            catch (Exception e)
            {
                LogError("Couldn't send message.", e.Message);
            }

            return null;
        }

        public static Task<IUserMessage> ReplyAsync(CommandArgs e, string text)
            => ReplyAsync(e.Author, e.Channel, text, true);
        public static Task<IUserMessage> ReplyAsync(CommandArgs e, string text, bool mentionUser)
            => ReplyAsync(e.Author, e.Channel, text, mentionUser);

        #endregion

        public static void LogError(string ErrorMessage, string exMessage)
        {
            StreamWriter sw = new StreamWriter(File.OpenWrite("./errorLog.txt"));
            sw.WriteLine($"[Error] {ErrorMessage} [Exception] {exMessage}");
            sw.Dispose();
        }

        #region server info
       
        public static ServerInfo GetServerInfo(ulong serverId)
        {
            if (Storage.serverInfo.ContainsKey(serverId))
                return Storage.serverInfo[serverId];
            else
            {
                var info = new ServerInfo();
                Storage.serverInfo.Add(serverId, info);
                return info;
            }
        }

        public static void SaveServerInfo()
        {
            StreamWriter sw = new StreamWriter(File.Open("./LocalFiles/ServerInfo.json", FileMode.OpenOrCreate));
            string json = JsonConvert.SerializeObject(Storage.serverInfo);
            sw.Write(json);
            sw.Dispose();
        }
        #endregion

        #region USER INFO
        public static SocketUser GetUser(CommandArgs eventArgs)
        {
            try
            {
                string userName = string.Empty;

                for (int i = 0; i < eventArgs.Args.Length - 1; i++)
                {
                    userName += eventArgs.Args[i] + ' ';
                }

                if (userName[0] == '@')
                    userName = userName.Substring(1);

                userName = userName.Remove(userName.Length - 1);

                var user = eventArgs.Guild.Users.FirstOrDefault(x => x.Username == userName);

                return user;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[EXCEPTION] Couldn't get user! {e.Message}");
                return null;
            }
        }

        public static int GetPerms(ulong serverId, IGuildUser u)
        {
            if (u.Id == (ulong)Storage.programInfo.DevID)
                return int.MaxValue;

            if (Storage.serverInfo.ContainsKey(serverId))
            {
                var surfer = Storage.serverInfo[serverId];

                int perm = 0;
                foreach (var role in u.RoleIds)
                {
                    if (surfer.roleImportancy.ContainsKey(role))
                    {
                        if (surfer.roleImportancy[role] > perm)
                            perm = surfer.roleImportancy[role];
                    }
                }
                return perm;
            }

            //returns -1 if failed or role has no tier set.
            return -1;
        }

        public static int GetPerms(CommandArgs e, IGuildUser u)
        {
            return GetPerms(e.Guild.Id, u);
        }
        #endregion

        #region StreamReader/Writer

        public static string ReadFile(string Path)
        {
            if (!File.Exists(Path))
                return null;

            using (StreamReader sr = new StreamReader(File.OpenRead(Path)))
            {
                return sr.ReadToEnd();
            }
        }

        public static void CreateFile(string Path)
        {
            File.Create(Path);
        }

        public static void SaveFile(string content, string Path, FileMode append)
        {
            using (StreamWriter sw = new StreamWriter(File.Open(Path, append)))
            {
                sw.WriteLine(content);
            }
        }

        #endregion

        public static bool InRange(double val, double min, double max)
        {
            if (val >= min && val <= max)
                return true; 
            return false;
        }
    }


}
