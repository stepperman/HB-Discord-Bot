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
    class ExperienceAdminCommands
    {
        [Command("xp ignorechannel", CommandType.Admin),
            Description("Toggle channel(s) between being ignored or not. Usage: `/xp ignorechannel #channel1 #channel2`."),
            Permission(Permission.OWNER)]
        public static async Task CmdEditIgnoreChannel(CommandArgs e)
        {
            if (e.Message.MentionedChannels.Count == 0)
                return;

            var serverinfo = Tools.GetServerInfo(e.Guild.Id);

            StringBuilder sBuilder = new StringBuilder();
            foreach (var channel in e.Message.MentionedChannels)
            {
                if (serverinfo.IgnoreChannels.Contains(channel.Id))
                {
                    serverinfo.IgnoreChannels.Remove(channel.Id);
                    sBuilder.AppendLine($"Removed channel #{channel.Name} from the ignored channels.");
                }
                else
                {
                    serverinfo.IgnoreChannels.Add(channel.Id);
                    sBuilder.AppendLine($"#{channel.Name} will now be ignored for collecting XP");
                }
            }

            await Tools.ReplyAsync(e, sBuilder.ToString());
            Tools.SaveServerInfo();
        }

        [Command("xp addrank", CommandType.Admin),
            Description("create or edit a rank Usage: `/xp addrank [XP] [role mention, name, or ID]`"),
            Args(ArgsType.ArgsAtLeast, 2),
            Permission(Permission.OWNER)]
        public static async Task CmdAddRank(CommandArgs e)
        {
            int xp;
            if(!Int32.TryParse(e.Args[0], out xp))
            {
                await Tools.ReplyAsync(e, "eh.. no.");
                return;
            }

            // find the role.
            IRole role = null;
            ulong roleId = 0;
            if (e.Message.MentionedRoles.Count > 0) // Find the role by mention
                role = e.Message.MentionedRoles.FirstOrDefault();
            else if (ulong.TryParse(e.Args[1], out roleId))   // Find role by ID
                role = e.Guild.GetRole(roleId);
            else        // Find role by name   
                role = e.Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains(e.Args[1].ToLower()));

            if (role == null)
            {
                await Tools.ReplyAsync(e, "No role found.");
                return;
            }

            // If XP is 0 or below, remove the rank instead.
            if(xp<=0)
            {
                RemoveRank(e.Guild.Id, role.Id);
                await Tools.ReplyAsync(e, $"Rank {role.Name} has been removed.");
                return;
            }

            AddUpdateRole(e.Guild.Id, role.Id, xp);
            await Tools.ReplyAsync(e, $"Role {role.Name} added/updated with {xp}");
        }

        public static void AddUpdateRole(ulong serverId, ulong roleId, int xp)
        {
            var serverinfo = Tools.GetServerInfo(serverId);

            var rankrolething = serverinfo.ServerRanks.FirstOrDefault(x => x.RoleID == roleId);
            if(rankrolething != null)
            {
                rankrolething.XP = xp;

                Tools.SaveServerInfo();
                return;
            }

            serverinfo.ServerRanks.Add(new Rank(xp, roleId, serverId));
            Tools.SaveServerInfo();
            ExperienceController.SetupServer();
            return;
        }

        [Command("xp removerank", CommandType.Admin),
            Description("remove a rank."),
            Permission(Permission.OWNER)]
        public static async Task CmdRemoveRank(CommandArgs e)
        {
            // find the role.
            IRole role = null;
            ulong roleId = 0;
            if (e.Message.MentionedRoles.Count > 0) // Find the role by mention
                role = e.Message.MentionedRoles.FirstOrDefault();
            else if (ulong.TryParse(e.Args[1], out roleId))   // Find role by ID
                role = e.Guild.GetRole(roleId);
            else        // Find role by name   
                role = e.Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains(e.ArgText.ToLower()));

            if (role == null)
                return;

            RemoveRank(e.Guild.Id, role.Id);
        }

        private static void RemoveRank(ulong serverId, ulong roleId)
        {
            var serverinfo = Tools.GetServerInfo(serverId);
            serverinfo.ServerRanks.RemoveAll(x => x.RoleID == roleId);
            Tools.SaveServerInfo();
            ExperienceController.SetupServer();
        }

        [Command("xp listranks", CommandType.Admin),
            Description("List all the ranks in the server")]
        public static async Task CmdListRanks(CommandArgs e)
        {
            var roles = ExperienceController.ServerRanks.Where(x => x.ServerRole == e.Guild.Id)
                .OrderBy(x => x.XP)
                .ToList();

            StringBuilder s = new StringBuilder();
            foreach(var role in roles)
            {
                var r = e.Guild.GetRole(role.RoleID);
                s.AppendLine(r.Name);
            }

            await Tools.ReplyAsync(e, s.ToString());
        }

        [Command("roleinfo", CommandType.Admin),
            Description("Get the info of a role.")]
        public static async Task CmdRoleInfo(CommandArgs e)
        {
            throw new NotImplementedException("This does absolute fuck all currently");
        }
    }
}
