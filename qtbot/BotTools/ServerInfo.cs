using System.Collections.Generic;
using System;

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

        //Regular User settings
        public bool RegularUsersEnabled = false;
        public ulong RegularUserRoleId = 0;
        public int RegularUserMinMessages = 25;
        public double RegularUserMinutesPerMessage = 5;
    }
}
