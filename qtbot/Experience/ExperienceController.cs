using Discord;
using qtbot.CommandPlugin;
using qtbot.CommandPlugin.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /*
            new Rank(20000, 277053512305737728, 99333280020566016),     //     Rank tier 1 
            new Rank(102000, 277053564747120640, 99333280020566016),    //     Rank tier 2
            new Rank(500, 277773089289273346, 99333280020566016)        //     User roles
        */

        public static async Task ReceivedMessageAsync(IMessage message)
        {
            if (!experienceSetup)
                SetupServer();

            //Check if it's in a guild channel
            var guildChannel = message.Channel as IGuildChannel;
            if (guildChannel == null)
                return;

            if (IgnoreChannels.Contains(guildChannel.Id) || 
                message.Author.Id == BotTools.Storage.client.CurrentUser.Id)
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
                if ((DateTime.Now - user.LastMessage).TotalMinutes < MessageCooldown || user.Excluded)
                    return;

                //Add XP
                var xp = getMessageXP(message.Content.Length);
                user.FullXP += xp;
                user.DisplayXP += xp;
                user.LastMessage = DateTime.Now;

                //Check and see if the tier 3 roles should be edited.
                await UpdateRoles(db, guildChannel.Guild.Id);

                //Try to see if this user is eligible for a new tier rank.
                var dRanks = ServerRanks
                    .Where(x => x.ServerRole == guildChannel.Guild.Id)
                    .OrderByDescending(x => x.XP)
                    .ToList() ;

                for(int i =0; i<dRanks.Count; i++)
                {
                    //If this user's XP is higher than the rank needed, give him the role. 
                    //(Unless he already has it)
                    if(user.FullXP > dRanks[i].XP)
                    {
                        var serverUser = await guildChannel.Guild.GetUserAsync(user.UserID);
                        if (serverUser.RoleIds.Contains(dRanks[i].RoleID))
                            continue;

                        var roles = serverUser.RoleIds.ToList();
                        roles.Add(dRanks[i].RoleID);

                        try
                        {
                            await serverUser.ModifyAsync(x => x.RoleIds = roles.ToArray());
                            await AnnounceNewRole(message, dRanks[i].RoleID);
                        }
                        catch(Exception) { }
                    }
                }

                db.Users.Update(user); //Update the user and save.
                await db.SaveChangesAsync();
            }
        }

        private static void SetupServer()
        {
            foreach(var server in BotTools.Storage.client.Guilds)
            {
                var serverInfo = BotTools.Tools.GetServerInfo(server.Id);
                
                foreach(var rank in serverInfo.ServerRanks)
                    ServerRanks.Add(rank);

                foreach(var ignoreChnl in serverInfo.IgnoreChannels)
                    IgnoreChannels.Add(ignoreChnl);
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

            await message.Channel.SendMessageAsync($"{message.Author.Mention} just ranked up! You've now reached **{role.Name}**. Congratulations!");
        }

        /// <summary>
        /// This will give the 10 most active people of the month a special rank.
        /// </summary>
        private static async Task UpdateRoles(ExperienceContext db, ulong ServerId)
        {

            var serverInfo = BotTools.Tools.GetServerInfo(ServerId);
            if (!serverInfo.RegularUsersEnabled || ServerId == 99333280020566016)
                return;

            //Return if the month is not NEW
            if (DateTime.Now.Month == serverInfo.month)
                return;

            serverInfo.month = DateTime.Now.Month;
            BotTools.Tools.SaveServerInfo(); //update & save the new month. 

            var top10users = db.Users
                .OrderByDescending(x => x.DisplayXP)
                .Where(x=>x.ServerID == ServerId)
                .Take(10)
                .ToList();

            ulong specialRole = 277054080839450625;

            foreach (var user in db.Users.ToList())
            {
                var serverUser = BotTools.Storage.client.GetGuild(ServerId).GetUser(user.UserID);

                //If the user is not in the top 10, but has the tier 3 role, remove it.
                if(!top10users.Contains(user) && serverUser.RoleIds.Contains(specialRole))
                {
                    var x = serverUser.RoleIds.ToList();
                    x.Remove(specialRole);
                    try
                    {
                        await serverUser.ModifyAsync(z => z.RoleIds = x.ToArray());
                    }
                    catch (Exception) { }
                }

                //If the user is in the top 10, check to see if he already has the role,
                //then add him.
                if (top10users.Contains(user))
                {
                    //If the user already has the role, there is no need to add it again.
                    if (serverUser.RoleIds.Contains(specialRole))
                        return;
                    var roles = serverUser.RoleIds.ToList();
                    roles.Add(specialRole); //add the new role to the role id list.
                    try
                    { 
                        await serverUser.ModifyAsync(z => z.RoleIds = roles.ToArray());
                    }
                    catch (Exception) { }
            }

                //reset the display XP for this month.
                user.LastResettedXP = DateTime.Now;
                user.DisplayXP = 0;
            }
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
