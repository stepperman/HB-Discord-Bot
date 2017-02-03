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
        [Command("top10"),
            Description("Get the monthly top 10 of all users on the current servers.")]
        public static async Task CmdGetTop10(CommandArgs e)
        {
            var db = new ExperienceContext();
                var users = db.Users
                    .OrderByDescending(x => x.DisplayXP)
                    .Where(x => x.ServerID == e.Guild.Id)
                    .ToList();
                string blah = await FormatList(users, e.Guild);
                await BotTools.Tools.ReplyAsync(e, String.IsNullOrEmpty(blah) ? "Couldn't make table" : blah);
            db.Dispose();
        }

        [Command("atop10"),
            Description("Get the top 10 of all time on the current server")]
        public static async Task CmdGetTopTop10(CommandArgs e)
        {
            using (var db = new ExperienceContext())
            {
                var users = db.Users
                    .OrderByDescending(x => x.FullXP)
                    .Where(x => x.ServerID == e.Guild.Id)
                    .ToList();
                
                string blah = await FormatList(users, e.Guild);
                await BotTools.Tools.ReplyAsync(e, String.IsNullOrEmpty(blah) ? "Couldn't make table" : blah);
            }
        }

        [Command("stats"),
            Description("Get your monthly XP, daily XP and XP needed to go to the next level.")]
        public static async Task CmdGetStats(CommandArgs e)
        {
            using (var db = new ExperienceContext())
            {

                var taggedUser = e.Message.MentionedUsers.Count > 0;
                ExperienceUser user;
                if (taggedUser)
                    user = db.Users.FirstOrDefault(x => e.Message.MentionedUsers.ToList()[0].Id == x.UserID);
                else
                    user = db.Users.FirstOrDefault(x => x.UserID == e.Author.Id);

                if(user == null)
                {
                    if (taggedUser)
                        await Tools.ReplyAsync(e, $"User {e.Message.MentionedUsers.ToList()[0].Username} doesn't have any stats.");
                    else
                        await Tools.ReplyAsync(e, "You don't have any stats."); //This should be impossible actually...

                    return;
                }

                EmbedBuilder embed = new EmbedBuilder();
                var serverUser = await e.Channel.GetUserAsync(user.UserID);

                if(serverUser == null)
                {
                    await e.Guild.DownloadUsersAsync();
                    serverUser = await e.Channel.GetUserAsync(user.UserID);

                    if(serverUser == null)
                    {
                        await Tools.ReplyAsync(e, "I couldn't find user with the ID of " + user.UserID + ". Fatal error");
                        return;
                    }
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

                var roles = ExperienceController.ServerRanks
                    .OrderBy(x => x.XP)
                    .Where(x => x.ServerRole == e.Guild.Id).ToList();

                int xp = 0;
                for(int i = 0; i < roles.Count; i++)
                {
                    if(user.FullXP < roles[i].XP)
                    {
                        xp = roles[i].XP;
                        break;
                    }
                }

                embed.AddField(x =>
                {
                    x.Name = "XP until next rank";
                    x.Value = (xp - user.FullXP).ToString();
                    x.IsInline = true;
                });

                await e.Channel.SendMessageAsync("", embed: embed);
            }
        }

        public static async Task<string> FormatList(List<ExperienceUser> users, IGuild guild)
        {
            StringBuilder msg = new StringBuilder($"Leaderboard for {guild.Name}\n```\nRank  |  Name\n");

            for(int i = 0; i < 10; i++)
            {
                if (i == users.Count)
                    break;

                var serveruser = await guild.GetUserAsync(users[i].UserID);
                string name = "User not found.";
                if (serveruser != null)
                    name = serveruser.Nickname == null ? serveruser.Username : serveruser.Nickname;

                msg.AppendLine($"#{i + 1}\t{name}");
                msg.AppendLine($"\t\tMonthly XP: {users[i].DisplayXP} Total XP: {users[i].DisplayXP}");
            }
            msg.Append("```");
            return msg.ToString();
        }
    }
}
