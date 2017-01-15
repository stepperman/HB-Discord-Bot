using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qtbot.CommandPlugin;
using qtbot.BotTools;
using qtbot.CommandPlugin.Attributes;
using Discord;

namespace qtbot.Modules
{
    public static class Uptime
    {
        private static DateTime dateTime = DateTime.Now;

        [Command("uptime"), 
            Description("Show the bot's uptime")]
        public static async Task ShowUptime(CommandArgs e)
        {
            DateTime currentTime = DateTime.Now;
            var uptime = currentTime - dateTime;

            await Tools.ReplyAsync(e, Tools.CalculateTimeWithSeconds((int)Math.Floor(uptime.TotalSeconds)));
        }
    }
}
