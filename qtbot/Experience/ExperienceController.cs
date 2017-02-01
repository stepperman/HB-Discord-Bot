using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qtbot.Experience
{
    class ExperienceController
    {
        public const int minXP = 8;
        public const int maxXP = 50;
        public const int XPPerChar = 2;
        public const int RoleUpdateRate = 50;
        public const double MessageCooldown = 5; //in minutes

        public static async Task ReceivedMessageAsync(IMessage message)
        {
            //Check if it's in a guild channel
            var guildChannel = message.Channel as IGuildChannel;
            if (guildChannel == null)
                return;

            using (var db = new ExperienceContext())
            {
                //Check if the user is in the database, if not, add him.
                var user = db.Users.FirstOrDefault(x => x.UserID == message.Author.Id && x.ServerID == guildChannel.Guild.Id);
                if(user == null)
                {
                    await CreateUser(message, guildChannel, db);
                    return; // return since no messages have to be sent anymore.
                }

                //If the user has sent a message before the cooldown was up, just return.
                

                //Add XP
                var xp = getMessageXP(message.Content.Length);
                user.FullXP += xp;
                user.DisplayXP += xp;


                db.Users.Update(user); //Update the user and save.
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// This will check all the users' DisplayXP and give them the appropriate role
        /// (This is only for the third role, meaning the top 10's heaviest talkers.)
        /// </summary>
        private static async Task UpdateRoles(ExperienceContext db)
        {

        }

        private static int getMessageXP(int messageCount)
        {
            var xp = messageCount * XPPerChar;
            if (xp > maxXP)
                return maxXP;
            else if (xp < minXP)
                return minXP;
            else
                return xp;
        }

        private static async Task CreateUser(IMessage message, IGuildChannel channel, ExperienceContext db)
        {
            ExperienceUser _temp = new ExperienceUser();
            _temp.FullXP = maxXP;
            _temp.DisplayXP = maxXP;
            _temp.LastMessage = DateTime.Now;
            _temp.LastResettedXP = DateTime.Now;
            _temp.UserID = message.Author.Id;
            _temp.ServerID = channel.Guild.Id;

            await db.Users.AddAsync(_temp);
        }
    }
}
