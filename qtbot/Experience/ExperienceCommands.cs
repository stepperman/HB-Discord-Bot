using Discord;
using qtbot.BotTools;
using qtbot.CommandPlugin;
using qtbot.CommandPlugin.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qtbot.Experience
{
    class ExperienceCommands
    {
        [Command("top10", alias: "top"),
            Description("Get the monthly top 10 of all users on the current servers.")]
        public static async Task CmdGetTop10(CommandArgs e)
        {
            var db = new ExperienceContext();
                var users = db.Users
                    .OrderByDescending(x => x.DisplayXP)
                    .Where(x => x.ServerID == e.Guild.Id)
                    .ToList();

            var currentUser = db.Users.FirstOrDefault(x => x.UserID == e.Author.Id && x.ServerID == e.Guild.Id);

            string blah = await FormatList(users, e.Guild, currentUser, GetPage(e.ArgText));

                await BotTools.Tools.ReplyAsync(e, String.IsNullOrEmpty(blah) ? "Couldn't make table" : blah);
            db.Dispose();
        }
        
        [Command("atop10", alias: "atop"),
            Description("Get the top 10 of all time on the current server")]
        public static async Task CmdGetTopTop10(CommandArgs e)
        {
            using (var db = new ExperienceContext())
            {
                var users = db.Users
                    .OrderByDescending(x => x.FullXP)
                    .Where(x => x.ServerID == e.Guild.Id)
                    .ToList();

                var currentUser = db.Users.FirstOrDefault(x => x.UserID == e.Author.Id && x.ServerID == e.Guild.Id);
                
                string blah = await FormatList(users, e.Guild, currentUser, GetPage(e.ArgText));
                await BotTools.Tools.ReplyAsync(e, String.IsNullOrEmpty(blah) ? "Couldn't make table" : blah);
            }
        }

        private static int GetPage(string text)
        {
            int page = 0;
            if (!Int32.TryParse(text, out page) || page < 0 || page > 10000000)
                return 1;
            return page;
        }
        
        [Command("stats", alias:"rank"),
            Description("Get your monthly XP, daily XP and XP needed to go to the next level.")]
        public static async Task CmdGetStats(CommandArgs e)
        {
            using (var db = new ExperienceContext())
            {

                var taggedUser = e.Message.MentionedUsers.Count > 0;
                ExperienceUser user;
                if (taggedUser)
                    user = db.Users.FirstOrDefault(x => e.Message.MentionedUsers.ToList()[0].Id == x.UserID && x.ServerID == e.Guild.Id);
                else
                    user = db.Users.FirstOrDefault(x => x.UserID == e.Author.Id && x.ServerID == e.Guild.Id);

                if(user == null || user.Excluded)
                {
                    if (taggedUser)
                        await Tools.ReplyAsync(e, $"User {e.Message.MentionedUsers.ToList()[0].Username} doesn't have any stats.");
                    else
                        await Tools.ReplyAsync(e, "You don't have any stats.");

                    return;
                }

                await BuildEmbed(e, user, db);
            }
        }

        /// <summary>
        /// Builds the embed and posts it to the channel.
        /// </summary>
        public static async Task BuildEmbed(CommandArgs e, ExperienceUser user, ExperienceContext db)
        {
            EmbedBuilder embed = new EmbedBuilder();

            if (RandomNumber.Next(3) == 0)
                await e.Guild.DownloadUsersAsync();

            var serverUser = await e.Channel.GetUserAsync(user.UserID);

            if (serverUser == null)
            {
                await e.Guild.DownloadUsersAsync();
                serverUser = await e.Channel.GetUserAsync(user.UserID);

                if (serverUser == null)
                {
                    await Tools.ReplyAsync(e, "I couldn't find user with the ID of " + user.UserID + ". Fatal error");
                    return;
                }
            }

            if(serverUser.IsBot)
            {
                await e.ReplyAsync("Bots do not have stats.");
                return;
            }

            embed.WithTitle(serverUser.Nickname == null ? serverUser.Username : serverUser.Nickname)
                .WithColor(new Color(80, 80, 180))
                .WithCurrentTimestamp()
                .WithThumbnailUrl(serverUser.AvatarUrl);

            embed.AddField(x =>
            {
                x.Name = "Current XP";
                x.Value = user.FullXP.ToString();
                x.IsInline = true;
            });

            embed.AddField(x =>
            {
                x.Name = "Monthly XP";
                x.Value = user.DisplayXP.ToString();
                x.IsInline = true;
            });

            //Get placing on the server
            var serverList = db.Users.Where(x => x.ServerID == e.Guild.Id)
                .OrderByDescending(x => x.DisplayXP)
                .ToList();

            int serverPlacing = -1;
            for (int i = 0; i < serverList.Count; i++)
            {
                if (serverList[i].UserID == user.UserID)
                {
                    serverPlacing = i + 1;
                    break;
                }
            }

            embed.AddField(x =>
            {
                x.Name = "Rank on server.";
                x.IsInline = true;
                x.Value = "#" + serverPlacing.ToString();
            });

            var roles = ExperienceController.ServerRanks
                .OrderBy(x => x.XP)
                .Where(x => x.ServerRole == e.Guild.Id).ToList();
            
            var nextRank = GetNextRank(user, e.Guild);

            if (nextRank != null)
            {
                var rankRole = e.Guild.GetRole(nextRank.RoleID);

                embed.AddField(x =>
                {
                    x.Name = "XP until " + rankRole.Name;
                    x.Value = (nextRank.XP - user.FullXP).ToString();
                    x.IsInline = true;
                });
            }

            await e.Channel.SendMessageAsync("", embed: embed);
        }

        public static Rank GetNextRank(ExperienceUser e, IGuild guild)
        {
            var ranks = ExperienceController.ServerRanks
                .Where(x => x.ServerRole == guild.Id)
                .OrderBy(x => x.XP).ToList();
            
            foreach(var rank in ranks)
            {
                if (rank.XP > e.FullXP)
                    return rank;
            }

            return null;
        }

        [Command("excludefromstats"),
            Description("Exclude yourself from stat collection, and from the top list.")]
        public static async Task CmdXPExclude(CommandArgs e)
        {
            using (var db = new ExperienceContext())
            {
                var user = db.Users.FirstOrDefault(x => x.UserID == e.Author.Id && e.Guild.Id == x.ServerID);
                if(user != null)
                    user.Excluded = !user.Excluded;
                db.Users.Update(user);
                await db.SaveChangesAsync();

                string newSetting = user.Excluded ? "excluded" : "included";
                await Tools.ReplyAsync(e, $"You are now {newSetting} from stats collection");
            }
        }

        public static async Task<string> FormatList(List<ExperienceUser> users, IGuild guild, ExperienceUser userID, int page)
        {
            StringBuilder msg = new StringBuilder($"Leaderboard for {guild.Name}. Page {page}.\n```Golo\n🏆 Rank | Name\n");

            for(int i = 0+(10*(page-1)); i < 10*page; i++)
            {
                if (i >= users.Count)
                    break;

                if (users[i].Excluded)
                    continue;

                var serveruser = await guild.GetUserAsync(users[i].UserID);
                string name = "User not found.";
                
                if(serveruser == null)
                {
                    try { await guild.DownloadUsersAsync(); } catch(Exception) { }
                    serveruser = await guild.GetUserAsync(users[i].UserID);
                }

                if(serveruser != null)
                    name = serveruser.Nickname == null ? serveruser.Username : serveruser.Nickname;


                msg.AppendLine(String.Format("{0,-6} {1,-15}", $"[{i+1}]", $"⇨ {name}"));
                msg.AppendLine(String.Format("\t\t{0,-20} {1,-20}", $"Monthly XP: { users[i].DisplayXP}", $"Total XP: {users[i].DisplayXP}"));
            }
            
             if(userID != null)
            {
                int userPlacing = 0;
                for(int i = 0;i<users.Count;i++)
                {
                    if (users[i].UserID == userID.UserID)
                    {
                        userPlacing = i + 1;
                        break;
                    }
                }
                msg.AppendLine(new string('-', 20));
                msg.AppendLine("@ Your placement on the server");
                msg.AppendLine($"Rank: {userPlacing}\tXP: {userID.FullXP}");
            }

            msg.Append("```");
            return msg.ToString();
        }
    }
}
