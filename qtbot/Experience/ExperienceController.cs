using Discord;
using qtbot.CommandPlugin;
using qtbot.CommandPlugin.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace qtbot.Experience
{
    class ExperienceController
    {
        private const int minXP = 8;
        private const int maxXP = 60;
        private const int XPPerChar = 1;
        private const int RoleUpdateRate = 50;
        private const double MessageCooldown = 0.7; //in minutes

        private static bool experienceSetup = false;

        public static List<ulong> IgnoreChannels { get; set; }
        public static List<Rank> ServerRanks { get; set; }

        public static async Task ReceivedMessageAsync(IMessage message)
        {
            if (!experienceSetup)
                SetupServer();

            //Check if it's in a guild channel
            var guildChannel = message.Channel as IGuildChannel;
            if (guildChannel == null)
                return;

            if ((IgnoreChannels.Count != 0) && IgnoreChannels.Contains(guildChannel.Id) || 
                message.Author.Id == BotTools.Storage.client.CurrentUser.Id ||
                message.Author.IsBot)
                return;

            using (var db = new ExperienceContext())
            {
                // Check if the user is in the database, if not, add him.
                var user = db.Users.FirstOrDefault(x => x.UserID == message.Author.Id && x.ServerID == guildChannel.Guild.Id);
                if(user == null)
                {
                    await CreateUser(message, guildChannel, db);
                    return; // return since no messages have to be sent anymore.
                }

                // If the user has sent a message before the cooldown was up, just return.
                if ((DateTime.Now - user.LastMessage).TotalMinutes < MessageCooldown || user.ExcludeFromStats)
                    return;

                // Add XP
                var xp = getMessageXP(message.Content.Length);
                user.FullXP += xp;
                user.DisplayXP += xp;
                user.LastMessage = DateTime.Now;


                if (ServerRanks.Count != 0)
                {
                    // Try to see if this user is eligible for a new tier rank.
                    var dRanks = ServerRanks
                        .Where(x => x.ServerRole == guildChannel.Guild.Id)
                        .OrderByDescending(x => x.XP)
                        .ToList();

                    var redeemableroles = db.Users_Redeem
                        .Where(x => x.UserID == message.Author.Id && x.ServerID == guildChannel.Guild.Id)
                        .ToList();

                    foreach (var rank in dRanks)
                    {
                        //If the role is already redeemable, continue.
                        if (redeemableroles.Any(x => x.RoleID == rank.RoleID))
                            continue;


                        if (user.FullXP < rank.XP)
                            continue;

                        db.Users_Redeem.Add(new UserRoleRedeem()
                        {
                            RoleID = rank.RoleID,
                            ServerID = rank.ServerRole,
                            UserID = message.Author.Id,
                            NeededXP = rank.XP
                        });
                        await AnnounceNewRole(message, rank.RoleID);
                    }
                }

                db.Users.Update(user); //Update the user and save.
                await db.SaveChangesAsync();
            }
        }

        public static void SetupServer()
        {
            IgnoreChannels = new List<ulong>();
            ServerRanks = new List<Rank>();

            if(BotTools.Storage.client == null)
                return;

            foreach(var server in BotTools.Storage.client.Guilds)
            {
                var serverInfo = BotTools.Tools.GetServerInfo(server.Id);

                foreach (var rank in serverInfo.ServerRanks)
                {
                    ServerRanks.Add(rank);
                    Console.WriteLine("Added " + rank.RoleID + ".");
                }

                foreach(var ignoreChnl in serverInfo.IgnoreChannels)
                {
                    IgnoreChannels.Add(ignoreChnl);
                    Console.WriteLine("Ignored " + ignoreChnl);
                }
            }

            experienceSetup = true;
        }

        /// <summary>
        /// Call this to announce it when someone gets a new role.
        /// </summary>
        private static async Task AnnounceNewRole(IMessage message, ulong roleID)
        {
            var guild = (message.Channel as IGuildChannel)?.Guild;
            var role = guild?.GetRole(roleID);

            if (role == null)
                return;

            await message.Channel.SendMessageAsync($"{message.Author.Mention} just ranked up! You can now equip **{role.Name}**. You can equip it with `/equip`.");
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
            await db.SaveChangesAsync();
        }
    }

    public class Rank
    {
        public int XP { get; set; }
        public ulong RoleID { get; set; }
        public ulong ServerRole { get; set; }

        /// <summary>
        /// Makes a new rank.
        /// </summary>
        /// <param name="xp">The XP required to reach this rank.</param>
        /// <param name="roleId">The ID of the role to give when the user reaches the required amount of XP</param>
        /// <param name="ServerId">The server associated with this role.</param>
        public Rank(int xp, ulong roleId, ulong ServerId)
        {
            XP = xp;
            RoleID = roleId;
            ServerRole = ServerId;
        }
    }
}
