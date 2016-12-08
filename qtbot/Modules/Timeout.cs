using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Threading;
using Discord;
using Discord.WebSocket;
using qtbot.CommandPlugin;

namespace qtbot.Modules
{
    class Timeout
    {
        Dictionary<ulong, List<TimedoutUser>> timedoutUsers = new Dictionary<ulong, List<TimedoutUser>>();
        IDiscordClient _client;

        public Timeout(IDiscordClient client)
        {
            SetupAsync(client).Wait();
        }

        private async Task SetupAsync(IDiscordClient client)
        {
            this._client = client;

            foreach (var server in await client.GetGuildsAsync())
            {
                timedoutUsers.Add(server.Id, new List<TimedoutUser>());
            }
        }

        public async Task<string> Admin_TimeoutUserAsync (CommandArgs e, double minutes, IGuildUser user)
        {
            var response = await TimeoutUserAsync(e, minutes, user);

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
        public async Task<int> TimeoutUserAsync(CommandArgs e, double minutes, IGuildUser user)
        {
            List<TimedoutUser> users;
            timedoutUsers.TryGetValue(e.Guild.Id, out users);

            if (users == null)
            {
                users = new List<TimedoutUser>();
                timedoutUsers.Add(e.Guild.Id, users);
            }

            var userTimeout = users.FirstOrDefault(x => x.userID  == user.Id);

            if (userTimeout == null)
            {
                if (minutes <= 0)
                    return 0; //Failed

                await StartTimeoutAsync(e, minutes, user, users);

                return 1;
            }
            else
            {
                if (minutes <= 0)
                {
                    await StopTimeoutAsync(users, userTimeout, user, e.Guild);
                    return 3;
                }

                var timeToAdd = (DateTime.Now - userTimeout.t).TotalMinutes;
                timeToAdd = userTimeout.timeoutTime - timeToAdd;

                Console.WriteLine($"{user.Username}'s timeout has been lengthed to {timeToAdd + minutes}");
                await StopTimeoutAsync(users, userTimeout, user, e.Guild);
                await StartTimeoutAsync(e, timeToAdd + minutes, user,  users);

                return 2; // Time added
            }
        }

        private async Task StartTimeoutAsync(CommandArgs e, double minutes, IGuildUser user, List<TimedoutUser> users)
        {
            users.Add(new TimedoutUser(user));
            var info = users[users.Count - 1];
            info.SetTimer(new TimeSpan(0, (int)minutes, (int)minutes % 60), new TimeoutInfo(users, info, user, user.Guild));

            timedoutUsers[user.GuildId] = new List<TimedoutUser>(users);
            
            info.t = DateTime.Now;
            info.timeoutTime = minutes;

            SocketRole role = null;
            var roles = e.Guild.Roles.ToArray();
            for(int i = 0; i < roles.Length; i++)
            {
                if(roles[i].Name == "qttimedout")
                {
                    role = roles[i];
                    break;
                }
            }

            if (role == null)
                return;

            var userroles = user.RoleIds.ToList();
            userroles.Add(role.Id);
            try
            {
                await user.ModifyAsync(x => x.RoleIds = userroles.ToArray());
            }
            catch (Exception) { }
            return;
        }

        public static async Task StopTimeoutAsync(List<TimedoutUser> users, TimedoutUser info, IGuildUser user, IGuild guild)
        {
            users.Remove(info);
            Console.WriteLine($"{user.Username}'s time out has been removed!");
            
            var role = guild.Roles.FirstOrDefault(x => x.Name == "qttimedout");
            var userroles = user.RoleIds.ToList();
            userroles.Remove(role.Id);
            try
            {
                await user.ModifyAsync(x => x.RoleIds = userroles.ToArray());
            }
            catch (Exception) { }

            info.timer.Dispose();
            return;
        }


    }

    struct TimeoutInfo
    {
        public TimeoutInfo(List<TimedoutUser> users, TimedoutUser info,
            IGuildUser user, IGuild guild)
        {
            this.users = users;
            this.info = info;
            this.user = user;
            this.guild = guild;
        }

        public List<TimedoutUser> users;
        public TimedoutUser info;
        public IGuildUser user;
        public IGuild guild;
    }

    class TimedoutUser
    {
        public TimedoutUser(IGuildUser user)
        {
            this.userID = user.Id;
        }

        public ulong userID;
        public double timeoutTime;
        public Timer timer;
        public DateTime t;

        public void SetTimer(TimeSpan time, object data)
        {
            timer = new Timer(async (r) =>
            {
                var x = (TimeoutInfo)r;
                await Timeout.StopTimeoutAsync(x.users, x.info, x.user, x.guild);

            }, data, new TimeSpan(0), time);
        }
    }
}
