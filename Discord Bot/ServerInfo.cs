using System;
using System.Collections.Generic;
using Discord;

namespace Discord_Bot
{
    class ServerInfo
    {
        public Dictionary<string, int> roleImportancy = new Dictionary<string, int>();
        public string standardRole;
        public ulong welcomingChannel;
        public int ayyScore = 0;
    }
}
