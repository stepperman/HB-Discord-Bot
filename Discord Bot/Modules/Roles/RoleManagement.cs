using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Discord_Bot.CommandPlugin;
using Discord;

namespace Discord_Bot
{
    public static class RoleManagement
    {
        private static string rolepath = "./../LocalFiles/roles.json";

        static RoleManagement()
        {
            if (!File.Exists(rolepath))
                File.Create(rolepath);
            }

        #region Commands

        public static Func<CommandArgs, Task> ShowRoles = async e =>
        {
            RoleType type = RoleType.Other;
            var msgtype = e.Args[0].ToLower();
            if (msgtype == "user")
            {
                await Tools.Reply(e, "The user category is too big to display it.");
                return;
            }
            else if (msgtype == "admin")
                type = RoleType.Admin;
            else if (msgtype == "other")
                type = RoleType.Other;

            var list = LoadRoleList(type);

            string msg = $"{type.ToString()} roles\n";
            foreach (var entry in list)
            {
                msg += $"`{entry.Key}`\n";
            }
            await Tools.Reply(e, msg);
        };

        public static Func<CommandArgs, Task> AddRole = async e =>
        {
            //Parse the role
            RoleType type = RoleType.Other;
            var msgtype = e.Args[e.Args.Count() - 1].ToLower();
            var args = e.Args.ToList();
            args.RemoveAt(args.Count() - 1);
            
            if (msgtype == "user")
                type = RoleType.User;
            else if (msgtype == "admin")
                type = RoleType.Admin;
            else if (msgtype == "other")
                type = RoleType.Other;

            var list = LoadRoleList(type);

            //Parse the role
            Role roleToAdd = e.Server.FindRoles(string.Join(" ", args)).FirstOrDefault();
            list.Add(roleToAdd.Name, new UserRole(roleToAdd.Id, type));
            SaveRoleList(list, type);

            await Tools.Reply(e, $"Added {roleToAdd.Name} to {type.ToString()}.");
        };

        #endregion

        private static Dictionary<string, UserRole> LoadRoleList(RoleType roleType)
        {
            if (!File.Exists(rolepath))
            {
                File.Create(rolepath);
                return new Dictionary<string, UserRole>();
            }
            
            var conv = LoadFullRoleList();
            Dictionary<string, UserRole> ass;
            if (conv.TryGetValue(roleType, out ass))
                return ass;
            else
                return new Dictionary<string, UserRole>();
        }

        private static Dictionary<RoleType, Dictionary<string, UserRole>> LoadFullRoleList()
        {
            var thing = JsonConvert.DeserializeObject<Dictionary<RoleType, Dictionary<string, UserRole>>>(File.ReadAllText(rolepath));
            if (thing == null)
                thing = new Dictionary<RoleType, Dictionary<string, UserRole>>();
            return thing;
        }

        private static void SaveRoleList(Dictionary<string, UserRole> list, RoleType roleToSave)
        {
            var a = LoadFullRoleList();
            a[roleToSave] = list;

            File.WriteAllText(rolepath, JsonConvert.SerializeObject(a,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
        }
    }
}
