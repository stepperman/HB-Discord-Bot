using System.Collections.Generic;

namespace Discord_Bot
{
    public class ServerInfo
    {
        public Dictionary<string, int> roleImportancy = new Dictionary<string, int>();
        public string standardRole;
        public ulong welcomingChannel;
        public int ayyScore = 0;
        public string safesearch = "medium";

        //Regular User settings
        public bool RegularUsersEnabled = false;
        public ulong RegularUserRoleId = 0;
        public int RegularUserMinMessages = 25;
    }
}
