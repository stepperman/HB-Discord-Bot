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

        public async Task<string> Admin_TimeoutUser (CommandArgs e, double minutes, User user)
        {
            var response = await TimeoutUser(e, minutes, user);

            if (response == 1)
                return $"timed out {user.Mention} for {minutes}";
            else if (response == 2)
                return $"added {minutes} more minutes to {user.Mention}'s timeout.";
            else if (response == 3)
                return $"removed {user.Mention}'s time out. Hooray!";
            else
                return $"failed to time out {user.Mention}. You might be a noob.";
        }

        /// <summary>
        /// Times out a user.
        /// </summary>
        /// <param name="minutes">How long to time the user out.</param>
        /// <param name="user">User to time out</param>
        /// <returns>0, 1, 2 or 3 if the timing out failed, succeeded, time was added or the time out was removed. Respectively.</returns>
        public async Task<int> TimeoutUser(CommandArgs e, double minutes, User user)
        {
            List<TimedoutUser> users;
            timedoutUsers.TryGetValue(e.Server.Id.ToString(), out users);

            var userTimeout = users.FirstOrDefault(x => x.userID == user.Id.ToString());

            if (userTimeout == null)
            {
                if (minutes <= 0)
                    return 0; //Failed
                users.Add(new TimedoutUser(user));
                var info = users[users.Count - 1];

                info.timer.Interval = minutes * 1000 * 60;
                info.t = DateTime.Now;
                info.timeoutTime = minutes;
                info.timer.Elapsed += async (s, te) =>
                {
                    users.Remove(info);
                    Console.WriteLine($"{e.User}'s time out has been removed!");
                    await user.Edit(null, null, null, info.roles);
                    info.timer = null;
                };
                await user.Edit(null, null, user.VoiceChannel, new Role[] { e.Server.EveryoneRole });
                info.timer.Start();
                return 1;
            }
            else
            {
                var info = userTimeout;
                if (minutes <= 0)
                {
                    users.Remove(info);
                    Console.WriteLine($"{e.User}'s time out has been removed!");
                    await user.Edit(null, null, user.VoiceChannel, info.roles);
                    return 3;//Timeout removed
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
                return 2; // Time added
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
