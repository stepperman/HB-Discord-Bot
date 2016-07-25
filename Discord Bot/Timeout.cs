using System.Collections.Generic;
using System.Timers;
using System.Linq;
using System;
using System.Threading.Tasks;
using Discord;
using Discord_Bot.CommandPlugin;

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
                return $"timed out {user.Mention} for {minutes} minutes.";
            else if (response == 2)
                return $"added {minutes} more minutes to {user.Mention}'s timeout.";
            else if (response == 3)
                return $"removed {user.Mention}'s time out. Hooray!";
            else
                return $"failed to time out {user.Mention}. You might stupid.";
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

            if (users == null)
            {
                users = new List<TimedoutUser>();
                timedoutUsers.Add(e.Server.Id.ToString(), users);
            }

            var userTimeout = users.FirstOrDefault(x => x.userID == user.Id.ToString());

            if (userTimeout == null)
            {
                if (minutes <= 0)
                    return 0; //Failed

                await StartTimeout(e, minutes, user, users);

                return 1;
            }
            else
            {
                if (minutes <= 0)
                {
                    await StopTimeout(users, userTimeout, user, e.Server);
                    return 3;
                }

                var timeToAdd = (DateTime.Now - userTimeout.t).TotalMinutes;
                timeToAdd = userTimeout.timeoutTime - timeToAdd;

                Console.WriteLine($"{user.Name}'s timeout has been lengthed to {timeToAdd + minutes}");
                await StopTimeout(users, userTimeout, user, e.Server);
                await StartTimeout(e, timeToAdd + minutes, user, users);

                return 2; // Time added
            }
        }

        private async Task StartTimeout(CommandArgs e, double minutes, User user, List<TimedoutUser> users)
        {
            users.Add(new TimedoutUser(user));
            var info = users[users.Count - 1];

            info.timer.Interval = minutes * 1000 * 60;
            info.t = DateTime.Now;
            info.timeoutTime = minutes;

            info.timer.Elapsed += async (s, te) =>
            {
                await StopTimeout(users, info, user, e.Server);
                return;
            };

            var role = e.Server.FindRoles("qttimedout").FirstOrDefault();
            var userroles = user.Roles.ToList();
            userroles.Add(role);
            try
            {
                await user.Edit(null, null, null, userroles);
            }
            catch (Exception) { }

            info.timer.Start();
            return;
        }

        private async Task StopTimeout(List<TimedoutUser> users, TimedoutUser info, User user, Server server)
        {
            users.Remove(info);
            Console.WriteLine($"{user.Name}'s time out has been removed!");

            var role = server.FindRoles("qttimedout").FirstOrDefault();
            var userroles = user.Roles.ToList();
            userroles.Remove(role);
            try
            {
                await user.Edit(null, null, null, userroles);
            }
            catch (Exception) { }

            info.timer.Dispose();
            return;
        }


    }

    class TimedoutUser
    {
        public TimedoutUser(User user)
        {
            this.userID = user.Id.ToString();
        }

        public string userID;
        public double timeoutTime;
        public Timer timer = new Timer();
        public DateTime t;
    }
}
