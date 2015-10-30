using System;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Games
{
    class GamePlugin
    {
        public List<Game> currentlyRunningGames = new List<Game>();

        public GamePlugin(DiscordClient client)
        {


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            client.MessageReceived += async (s, e) =>
            {
                //Don't bother processing if there's no game running.
                if (currentlyRunningGames.Count == 0)
                    return;

                //Ignore ourselves
                if (e.User.Id == client.CurrentUserId)
                    return;

                //Don't bother prcessing if the playing players are not playing any games
                if (!currentlyRunningGames.Any(r => r.User1 == e.User) && !currentlyRunningGames.Any(r => r.User2 == e.User))
                    return;
            };
        }
    }
}
