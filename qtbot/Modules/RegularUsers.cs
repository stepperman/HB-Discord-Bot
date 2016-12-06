using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qtbot.BotTools;
using Discord;
using System.IO;
using Newtonsoft.Json;  

namespace qtbot.Modules
{
    //TODO: CHANGE SO IT WORKS DIFFERENTLY.
    public static class RegularUsers
    {
        private static Dictionary<ulong, List<UserInfo>> info = new Dictionary<ulong, List<UserInfo>>();
        private const string filelocation = "../LocalFiles/regusr.json";
        private static int currentMessages = 0;
        private const int messagesBeforeSave = 50;
        private const ulong hour = 3600000;
        
        public static async Task ReceivedMessageAsync(IMessage e)
        {
            var serverId = (e.Channel as ITextChannel).GuildId;

            //If it does not contain the server key yet, add it.
            if (!info.ContainsKey(serverId))
                info.Add(serverId, new List<UserInfo>());

            List<UserInfo> temp;
            if (info.TryGetValue(serverId, out temp))
            {
                var usr = temp.FirstOrDefault(x => x.id == e.Author.Id);

                //If the user doesn't exist, create it.
                if (usr == null)
                {
                    usr = new UserInfo() { id = e.Author.Id, messageCount = 1, lastMessage = DateTime.Now, firstMessage = DateTime.Now };
                    temp.Add(usr); //Add the user to the list.
                }

                if ((DateTime.Now - usr.lastMessage).TotalMinutes >= Tools.GetServerInfo(serverId).RegularUserMinutesPerMessage)
                {
                    usr.lastMessage = DateTime.Now;
                    usr.messageCount++;
                }
            }

            currentMessages++;
            if (currentMessages >= messagesBeforeSave)
            {
                currentMessages = 0;
                await SaveAsync();
            }
        }

        public static async Task SaveAsync()
        {
            if (File.Exists(filelocation))
            {
                await SaveExistsAsync();
            }
            else
            {
                var temp = JsonConvert.SerializeObject(info);
                var sr = File.CreateText(filelocation);
                sr.Write(temp);
                sr.Dispose();

                info.Clear();
            }
        }

        private static async Task SaveExistsAsync()
        {
            var temp = JsonConvert.DeserializeObject<Dictionary<ulong, List<UserInfo>>>(File.ReadAllText(filelocation));

            //Add the servers that are non existing.
            foreach (var srvr in info)
            {
                if (!temp.ContainsKey(srvr.Key))
                {
                    temp.Add(srvr.Key, srvr.Value);
                    continue;
                }

                var tempsever = temp[srvr.Key];

                if (tempsever.Count <= 0)
                    return;

                //Add all non existing users
                foreach (var usr in srvr.Value)
                {
                    if (!tempsever.Any(x => x != null && x.id == usr.id))
                        tempsever.Add(usr);
                }
            }

            foreach (var server in temp)
            {
                for (int i = 0; i < server.Value.Count; i++)
                {
                    if (server.Value[i] == null)
                        continue;

                    try { server.Value[i] = await ProcessUserAsync(server.Key, server.Value[i]); } catch (Exception) { }
                }
            }

            //Save the fucking shit
            var jsonstring = JsonConvert.SerializeObject(temp);
            File.WriteAllText(filelocation, jsonstring);
            
            info.Clear();
        }

        private static async Task<UserInfo> ProcessUserAsync(ulong server, UserInfo user)
        {
            List<UserInfo> serverId;
            if (info.TryGetValue(server, out serverId))
            {
                var usr = serverId.FirstOrDefault(x => user != null && x != null && x.id == user.id);
                var serverinfo = Tools.GetServerInfo(server);

                //Remove the regular amount of messages if more than 4 days have passed.
                if ((DateTime.Now - user.firstMessage).TotalDays >= 4)
                {
                    user.messageCount -= serverinfo.RegularUserMinMessages * 3;
                    if (user.messageCount < 0)
                        user.messageCount = 0;
                    user.firstMessage = DateTime.Now;
                }

                //mix the users
                if (usr != null)
                {
                    user.lastMessage = usr.lastMessage;
                    user.messageCount += usr.messageCount;
                }


                try
                {

                    //Check if user should get the role
                    var svr = Storage.client.Guilds.FirstOrDefault(x => x.Id == server);
                    var usrmodel = svr.GetUser(user.id);


                    if (user.messageCount >= serverinfo.RegularUserMinMessages)
                    {
                        //Get/Keep the role
                        var role = svr.GetRole(serverinfo.RegularUserRoleId);

                        if (!usrmodel.RoleIds.Contains(role.Id))
                        {
                            try
                            {
                                var userRoles = usrmodel.RoleIds.ToList();
                                userRoles.Add(role.Id);
                                await usrmodel.ModifyAsync(x => x.RoleIds = userRoles.ToArray());
                            }
                            catch (Exception) { Console.WriteLine($"Couldn't edit {usrmodel.Username}"); }
                        }
                    }
                    else
                    {
                        //Lose the role
                        var role = svr.GetRole(serverinfo.RegularUserRoleId);

                        if (usrmodel.RoleIds.Contains(role.Id))
                        {
                            try
                            {
                                var userRoles = usrmodel.RoleIds.ToList();
                                userRoles.Remove(role.Id);
                                await usrmodel.ModifyAsync(x => x.RoleIds = userRoles.ToArray());
                            }
                            catch (Exception) { Console.WriteLine($"Couldn't edit {usrmodel.Username}"); }
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Couldn't edit a user. Possibly not on the server anymore?");
                }

                return user;
            }

            return null;
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
