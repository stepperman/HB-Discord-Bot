using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord_Bot.CommandPlugin;
using Discord;

namespace Discord_Bot
{
    public static class Uptime
    {
        private static DateTime dateTime = DateTime.Now;

        public static Func<CommandArgs, Task> ShowUptime = async e =>
        {
            DateTime currentTime = DateTime.Now;
            var uptime = currentTime - dateTime;

            await Tools.Reply(e, Tools.CalculateTimeWithSeconds((int)Math.Floor(uptime.TotalSeconds)));
        };
    }
}
