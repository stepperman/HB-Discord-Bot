using System.Collections.Generic;
using System.IO;
using Discord;

namespace Discord_Bot
{
    static class Storage
    {
        public static string UserSettingsPath = "../LocalFiles/killscore.json";

        static Storage()
        {
            if (!File.Exists(UserSettingsPath))
            {
                Tools.CreateFile(UserSettingsPath);
                Storage.UserInfo = new Dictionary<ulong, Models.UserSetting>();
            }
            else
            {
                string json = Tools.ReadFile(UserSettingsPath);
                Storage.UserInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, Models.UserSetting>>(json);
                if (Storage.UserInfo == null)
                    Storage.UserInfo = new Dictionary<ulong, Models.UserSetting>();
            }
        }

        public static DiscordClient client;
        public static Dictionary<string, ServerInfo> serverInfo = new Dictionary<string, ServerInfo>();
        public static int ayyscore = 0;
        public static Dictionary<ulong, List<msg>> UserMentions = new Dictionary<ulong, List<msg>>();
        public static dynamic programInfo = null;

        public struct msg
        {
            public string Author;
            public string Message;
        }

        public static Dictionary<ulong, Models.UserSetting> UserInfo { get; private set; }

        public static Models.UserSetting GetUser(ulong id)
        {
            Models.UserSetting user;
            if (!UserInfo.TryGetValue(id, out user))
            {
                user = new Models.UserSetting();
                UserInfo.Add(id, user);
            }

            return user;
        }

        public static void CheckUser(ulong id)
        {
            Models.UserSetting user;
            if (!UserInfo.TryGetValue(id, out user))
            {
                user = new Models.UserSetting();
                UserInfo.Add(id, user);
            }
        }

        public static void SaveUserSettings()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(UserInfo);
            Tools.SaveFile(json, Storage.UserSettingsPath, false); //Save it to disk.
        }

    }
}
