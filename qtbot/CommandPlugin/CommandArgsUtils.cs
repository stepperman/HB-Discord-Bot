using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace qtbot.CommandPlugin
{
    public static class CommandArgsUtils
    {
        public static async Task<Discord.IUserMessage> ReplyAsync(this CommandArgs e, string text, bool mention = true)
            => await BotTools.Tools.ReplyAsync(e, text, mention);
    }
}
