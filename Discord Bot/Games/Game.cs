using System;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Games
{
    class Game
    {
        public User User1 { get; private set; }
        public User User2 { get; private set; }

        internal Game(User gameCreator, User userPlaying)
        {
            User1 = gameCreator;
            User2 = userPlaying;
        }

    }
}
