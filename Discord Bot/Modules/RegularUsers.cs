using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord_Bot.CommandPlugin;
using Discord;
using System.IO;
using System.Timers;
using Newtonsoft.Json;  

namespace Discord_Bot
{
    public static class RegularUsers
    {
        public static Dictionary<ulong, List<UserInfo>> info = new Dictionary<ulong, List<UserInfo>>();
        public static readonly string filelocation = "../LocalFiles/regusr.json";
        public static Timer saveTimer;
        public const ulong hour = 3600000;

        static RegularUsers()
        {
            //Create the timers and start it.
            saveTimer = new Timer(hour/4);
            
            saveTimer.Elapsed += async (se, e) => 
            {
                await Save();
                Console.WriteLine("Saved with timer");
            };

            saveTimer.Start();
        }

        public static async Task ReceivedMessage(MessageEventArgs e)
        {
            //If it does not contain the server key yet, add it.
            if (!info.ContainsKey(e.Server.Id))
                info.Add(e.Server.Id, new List<UserInfo>());

            List<UserInfo> temp;
            if (info.TryGetValue(e.Server.Id, out temp))
            {
                var usr = temp.FirstOrDefault(x => x.id == e.User.Id);

                //If the user doesn't exist, create it.
                if (usr == null)
                {
                    usr = new UserInfo() { id = e.User.Id, messageCount = 1, lastMessage = DateTime.Now, firstMessage = DateTime.Now };
                    temp.Add(usr); //Add the user to the list.
                }

                if ((DateTime.Now - usr.lastMessage).TotalMinutes >= 0.01)
                {
                    usr.lastMessage = DateTime.Now;
                    usr.messageCount++;
                }
            }

            
        }

        public static async Task Save()
        {
            if (File.Exists(filelocation))
            {
                await SaveExists();
            }
            else
            {
                var temp = JsonConvert.SerializeObject(info);
                var sr = File.CreateText(filelocation);
                sr.Write(temp);
                sr.Close();

                info.Clear();
                saveTimer.Stop();
                saveTimer.Start();
            }
        }

        private static async Task SaveExists()
        {
            var temp = JsonConvert.DeserializeObject<Dictionary<ulong, List<UserInfo>>>(File.ReadAllText(filelocation));

            //Add the servers that are non existing.
            foreach (var srvr in info)
            {
                if (!temp.ContainsKey(srvr.Key))
                    temp.Add(srvr.Key, srvr.Value);

                var tempsever = temp[srvr.Key];

                //Add all non existing users
                foreach (var usr in srvr.Value)
                {
                    if (tempsever.Contains(usr))
                        continue;

                    tempsever.Add(usr);
                }
            }

            foreach (var server in temp)
            {
                foreach (var user in server.Value)
                {
                    await ProcessUser(server.Key, user, temp);
                }
            }

            //Save the fucking shit
            var jsonstring = JsonConvert.SerializeObject(temp);
            File.WriteAllText(filelocation, jsonstring);
            
            info.Clear();
            saveTimer.Stop();
            saveTimer.Start();
        }

        private static async Task ProcessUser(ulong server, UserInfo user, Dictionary<ulong, List<UserInfo>> temp)
        {
            List<UserInfo> serverId;
            if (info.TryGetValue(server, out serverId))
            {
                var usr = serverId.FirstOrDefault(x => x.id == user.id);
                var serverinfo = Tools.GetServerInfo(server);

                //Remove the regular amount of messages if more than 4 days have passed.
                if ((DateTime.Now - user.firstMessage).TotalDays >= 4)
                {
                    user.messageCount -= serverinfo.RegularUserMinMessages;
                    user.firstMessage = DateTime.Now;
                }

                //mix the users
                if (usr != null)
                {
                    user.lastMessage = usr.lastMessage;
                    user.messageCount += usr.messageCount;
                }



                //Check if user should get the role
                var svr = Storage.client.GetServer(server);
                var usrmodel = svr.GetUser(user.id);

                if (user.messageCount >= serverinfo.RegularUserMinMessages)
                {
                    //Get/Keep the role
                    var role = svr.GetRole(serverinfo.RegularUserRoleId);

                    if (!usrmodel.HasRole(role))
                    {
                        try
                        {
                            var userRoles = usrmodel.Roles.ToList();
                            userRoles.Add(role);
                            await usrmodel.Edit(null, null, null, userRoles);
                        }
                        catch (Exception) { Console.WriteLine($"Couldn't edit {usrmodel.Name}"); }
                    }
                }
                else
                {
                    //Lose the role
                    var role = svr.GetRole(serverinfo.RegularUserRoleId);

                    if (usrmodel.HasRole(role))
                    {
                        try
                        {
                            var userRoles = usrmodel.Roles.ToList();
                        userRoles.Remove(role);
                        await usrmodel.Edit(null, null, null, userRoles);
                        }
                        catch (Exception) { Console.WriteLine($"Couldn't edit {usrmodel.Name}"); }
                }
                }
            }
        }

        public class UserInfo
        {
            public ulong id;
            public int messageCount;
            public DateTime lastMessage;
            public DateTime firstMessage;
        }
    }


}
