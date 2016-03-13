using System.Collections.Generic;
using System.Timers;
using System.Linq;
using System;
using System.Threading.Tasks;
using Discord;
using Discord_Bot.Commands;

namespace Discord_Bot
{
    class Timeout
    {
        Dictionary<string, List<TimedoutUser>> timedoutUsers = new Dictionary<string, List<TimedoutUser>>();
        DiscordClient _client;

        public Timeout(DiscordClient client)
        {
            this._client = client;

            foreach(var server in client.Servers)
            {
                timedoutUsers.Add(server.Id.ToString(), new List<TimedoutUser>());
            }
        }

        public async Task<string> TimeoutUser (CommandArgs e, double minutes, User user)
        {
            List<TimedoutUser> users;
            timedoutUsers.TryGetValue(e.Server.Id.ToString(), out users);

            var userTimeout = users.FirstOrDefault(x => x.userID == user.Id.ToString());

            if(userTimeout == null)
            {
                if (minutes <= 0)
                {
                    return $"You can't timeout someone for negative minutes!";
                }
                users.Add(new TimedoutUser(user));
                var info = users[users.Count - 1];
                
                info.timer.Interval = minutes * 1000 * 60;
                info.t = DateTime.Now;
                info.timeoutTime = minutes;
                info.timer.Elapsed += async (s, te) =>
                {
                    users.Remove(info);
                    await user.Edit(null, null, null, info.roles);
                };
                await user.Edit(null, null, user.VoiceChannel, new Role[] { e.Server.EveryoneRole });
                info.timer.Start();
                return $"timed out {user.Mention} for {minutes} minutes.";
            }
            else
            {
                var info = userTimeout;
                if (minutes <= 0)
                {
                    users.Remove(info);
                    await user.Edit(null, null, user.VoiceChannel, info.roles);
                    return $"removed {user.Mention}'s timeout! Hooray!";
                }

                var timeToAdd = (DateTime.Now - info.t).TotalMinutes;
                timeToAdd = info.timeoutTime - timeToAdd;

                info.t = DateTime.Now;
                info.timer = new Timer((timeToAdd + minutes) * 1000 * 60);

                info.timer.Elapsed += async (s, te) =>
                {
                    users.Remove(info);
                    await user.Edit(null, null, user.VoiceChannel, info.roles);
                    info.timer = null;

                };
                info.timer.Start();
                return $"added {minutes} minutes to {user.Mention}'s timeout. Totalling to a timeout of {Math.Round(timeToAdd + minutes).ToString()} minutes!";
            }
            
        }
    }

    class TimedoutUser
    {
        public TimedoutUser(User user)
        {
            this.userID = user.Id.ToString();
            this.roles = user.Roles.ToArray();
        }

        public string userID;
        public double timeoutTime;
        public Role[] roles;
        public Timer timer = new Timer();
        public DateTime t;
    }
}
