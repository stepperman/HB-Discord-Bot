using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using System.IO;
using Discord_Bot.CommandPlugin;
using Newtonsoft.Json;

namespace Discord_Bot
{
    static class Tools
    {
        
        public static Random random = new Random();
        
        static Tools()
        {
            if (File.Exists("../LocalFiles/ServerInfo.json"))
            {
                var sw = new StreamReader("../LocalFiles/ServerInfo.json");

                string json = sw.ReadToEnd();
                Storage.serverInfo = JsonConvert.DeserializeObject<Dictionary<string, ServerInfo>>(json);
                sw.Close();
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
        public static async Task<Message> Reply(User user, Channel channel, string text, bool mentionUser)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (channel.IsPrivate || !mentionUser)
                        return await channel.SendMessage(text);
                    else
                        return await channel.SendMessage($"{user.Mention}: {text}");
                }
            }
            catch (Exception e)
            {
                LogError("Couldn't send message.", e.Message);
            }

            return null;
        }

        public static Task<Message> Reply(CommandArgs e, string text)
            => Reply(e.User, e.Channel, text, true);
        public static Task<Message> Reply(CommandArgs e, string text, bool mentionUser)
            => Reply(e.User, e.Channel, text, mentionUser);

        #endregion

        public static void LogError(string ErrorMessage, string exMessage)
        {
            StreamWriter sw = new StreamWriter("./errorLog.txt", true);
            sw.WriteLine($"[Error] {ErrorMessage} [Exception] {exMessage}");
            sw.Close();
        }

        #region server info
       
        public static ServerInfo GetServerInfo(ulong serverId)
        {
            if (Storage.serverInfo.ContainsKey(serverId.ToString()))
                return Storage.serverInfo[serverId.ToString()];
            else
            {
                var info = new ServerInfo();
                Storage.serverInfo.Add(serverId.ToString(), info);
                return info;
            }
        }

        public static void SaveServerInfo()
        {
            StreamWriter sw = new StreamWriter("../LocalFiles/ServerInfo.json", false);
            string json = JsonConvert.SerializeObject(Storage.serverInfo);
            sw.Write(json);
            sw.Close();
        }
        #endregion

        #region USER INFO
        public static User GetUser(CommandArgs eventArgs)
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

                var user = eventArgs.Server.FindUsers(userName).FirstOrDefault();

                return user;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[EXCEPTION] Couldn't get user! {e.Message}");
                return null;
            }
        }

        public static int GetPerms(string serverId, User u)
        {
            if (u.Id.ToString() == Program.ProgramInfo.DevID.ToString())
                return 100000;

            if (Storage.serverInfo.ContainsKey(serverId))
            {
                var surfer = Storage.serverInfo[serverId];

                foreach (var role in u.Roles)
                {
                    if (surfer.roleImportancy.ContainsKey(role.Id.ToString()))
                    {
                        return surfer.roleImportancy[role.Id.ToString()];
                    }
                }
            }

            //returns -1 if failed or role has no tier set.
            return -1;
        }

        public static int GetPerms(CommandArgs e, User u)
        {
            return GetPerms(e.serverId, u);
        }
        #endregion

        #region StreamReader/Writer

        public static string ReadFile(string Path)
        {
            if (!File.Exists(Path))
                return null;

            using (StreamReader sr = new StreamReader(Path))
            {
                return sr.ReadToEnd();
            }
        }

        public static void CreateFile(string Path)
        {
            File.Create(Path);
        }

        public static void SaveFile(string content, string Path, bool append)
        {
            using (StreamWriter sw = new StreamWriter(Path, append))
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
