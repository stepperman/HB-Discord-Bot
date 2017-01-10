using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace qtbot.Modules.Timeout
{
    public static class TimeoutController
    {
        public static Dictionary<ulong, ServerTimeout> servers = new Dictionary<ulong, ServerTimeout>();

        public static async Task AddNewTimeout(ITextChannel channel, IGuildUser user, double minutes)
        {
            ServerTimeout s;
            if (!servers.ContainsKey(channel.Guild.Id))
                servers.Add(channel.Guild.Id, new ServerTimeout(channel.Guild));
            s = servers[channel.Guild.Id];

            await s.TimeoutUser(user, minutes);
        }
    }

    public class ServerTimeout
    {
        uint usersTimedout = 0;
        ulong timeoutRole;
        public List<TimedoutUser> usersTimedOut = new List<TimedoutUser>();

        public ServerTimeout(IGuild guild)
        {
            var role = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "qttimedout");
            if (role != null)
                timeoutRole = role.Id;
        }

        public async Task<TimeoutResult> TimeoutUser(IGuildUser user, double minutes)
        {
            //Check if the user is already timed out, if so, lengthen in it.

            return TimeoutResult.FAILED;
        }

        public async Task<TimeoutResult> RemoveTimeout(ulong userId, ulong guildId)
        {
            //Check if the user is already timed out, if so, remove it.
            ServerTimeout x;
            if (!TimeoutController.servers.TryGetValue(guildId, out x))
                return TimeoutResult.FAILED;

            return TimeoutResult.FAILED;
        }

        public enum TimeoutResult
        {
            FAILED,
            SUCCESS,
            INCREASED,
            REMOVED
        }
    }

    public class TimedoutUser
    {
        public TimedoutUser(IGuildUser user, TimeSpan time)
        {
            timeoutStart = DateTime.Now;
            guildUser = user;
            timer = new Timer((x) => RemoveTimeout(x).Wait(), this, new TimeSpan(0), time);
        }

        public DateTime timeoutStart;
        public ServerTimeout s;
        public Timer timer;
        public IGuildUser guildUser;
        
        public async Task RemoveTimeout(object data)
        {
            var x = data as TimedoutUser;
            var c =  await x?.s.RemoveTimeout(x.guildUser.Id, x.guildUser.Guild.Id);
                
                
        }
    }
}
