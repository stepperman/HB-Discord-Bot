using System.Collections.Generic;
using System.IO;
using Discord;
using Discord.WebSocket;
using System;

namespace qtbot.BotTools
{
    static class Storage
    {
        public static string UserSettingsPath = "../LocalFiles/killscore.json";

        public static DiscordSocketClient client;
        public static Dictionary<ulong, ServerInfo> serverInfo = new Dictionary<ulong, ServerInfo>();
        public static int ayyscore = 0;
        public static Dictionary<ulong, List<Msg>> UserMentions = new Dictionary<ulong, List<Msg>>();
        public static dynamic programInfo = null;


        //Anilist
        public static string anilistAccessToken = "";
        public static DateTime anilistAuthorizationCreated;

        public struct Msg
        {
            public string Author;
            public string Message;
        }

    }
}
