using System.Collections.Generic;
using System;
using qtbot.Experience;

namespace qtbot.BotTools
{
    public class ServerInfo
    {
        public Dictionary<ulong, int> roleImportancy = new Dictionary<ulong, int>();
        public ulong standardRole;
        public ulong welcomingChannel;
        public int ayyScore = 0;
        public int ayyHighScore = 0;
        public DateTime ayyScoreDateReached = DateTime.Now.AddDays(-1);
        public string safesearch = "medium";

        //XP settings
        public bool RegularUsersEnabled = false;
        public int month = -1;

        public List<ulong> IgnoreChannels { get; set; }
        public List<Rank> ServerRanks { get; set; }
    }
}
