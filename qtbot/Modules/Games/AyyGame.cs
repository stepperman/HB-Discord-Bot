using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using qtbot.BotTools;

namespace qtbot.Modules.Games
{
    public static class AyyGame
    {
        public static async Task GameAsync(SocketMessage e)
        {
            if (e.Channel.Id == 134267667245694976 && Storage.ayyscore > 0)
            {
                try
                {
                    string msg = e.Content;
                    if (msg[0] == '/')
                        msg = msg.Substring(1);

                    if (!msg.ToLower().Replace(" ", "").EndsWith("ayy"))
                    {
                        var info = Tools.GetServerInfo((e.Channel as ITextChannel).GuildId);
                        string text = "get as long a chain of /ayy 's before it gets broken. High Score: {0} Current Score: {1}";
                        var oldayy = Storage.ayyscore;

                        bool newHighScore = Storage.ayyscore > info.ayyScore;
                        info.ayyScore = oldayy;

                        Storage.ayyscore = 0;
                        await Tools.ReplyAsync(e.Author, e.Channel, $"You failed! The chain has been broken. The score was: {oldayy}", true);

                        if(newHighScore)
                        {
                            string date = Tools.CalculateTime((int)(DateTime.Now - info.ayyScoreDateReached).TotalMinutes);
                            info.ayyScoreDateReached = DateTime.Now;
                            await Tools.ReplyAsync(e.Author, e.Channel, $"A new high score has been reached after {date}. The new highscore is {oldayy}", false);
                        }
                        
                        await (e.Channel as ITextChannel)?.ModifyAsync(x => { x.Topic = String.Format(text, info.ayyScore, Storage.ayyscore); });
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
