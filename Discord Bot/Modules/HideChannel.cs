using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Modules
{
    public static class HideChannel
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static Func<CommandPlugin.CommandArgs, Task> Hide = async e =>
        {
            var userInfo = Storage.GetUser(e.User.Id);

            if (!userInfo.HiddenChannels.ContainsKey(e.Server.Id))
            {
                userInfo.HiddenChannels.Add(e.Server.Id, new List<ulong>());
            }

            var hiddenChannels = userInfo.HiddenChannels[e.Server.Id];
            var mentionedChannels = e.Message.MentionedChannels;

            //fuck off if no mentioned channels. I mean, you need that shit in your message!!
            if (mentionedChannels.Count() == 0)
                return;

            foreach (var channel in mentionedChannels)
            {
                if (hiddenChannels.Contains(channel.Id))
                    return;

                try
                {
                    await channel.AddPermissionsRule(e.User, new Discord.ChannelPermissionOverrides(null,
                    null, Discord.PermValue.Deny, Discord.PermValue.Deny, null, null, null, null, Discord.PermValue.Deny, Discord.PermValue.Deny));

                    hiddenChannels.Add(channel.Id);
                }
                
                catch (Exception) { }

                Storage.SaveUserSettings();
            }
        };

        public static Func<CommandPlugin.CommandArgs, Task> Show = async e =>
        {
            var userInfo = Storage.GetUser(e.User.Id);

            if (!userInfo.HiddenChannels.ContainsKey(e.Server.Id))
            {
                userInfo.HiddenChannels.Add(e.Server.Id, new List<ulong>());
            }

            var hiddenChannels = userInfo.HiddenChannels[e.Server.Id];

            //blah
            if (e.Args[0] == string.Empty)
                return;

            int n;
            bool parsed = int.TryParse(e.Args[0], out n);

            if (parsed)
            {
                if (n < 0 || n >= hiddenChannels.Count())
                    return;

                await e.Server.GetChannel(hiddenChannels[n]).RemovePermissionsRule(e.User);

                hiddenChannels.RemoveAt(n);

                Storage.SaveUserSettings();
            }
        };

        public static Func<CommandPlugin.CommandArgs, Task> List = async e =>
        {
            var userInfo = Storage.GetUser(e.User.Id);

            if (!userInfo.HiddenChannels.ContainsKey(e.Server.Id))
                return;

            if (userInfo.HiddenChannels[e.Server.Id].Count() == 0)
                return;

            string show = "hidden channels:\n";
            for (int i = 0; i < userInfo.HiddenChannels[e.Server.Id].Count(); i++)
            {
                show += $"{i} : #{e.Server.GetChannel(userInfo.HiddenChannels[e.Server.Id][i]).Name}\n";
            }

            show += "You can type `/showchannel {channelnum}` or `/hidechannel #{channel}` to show/hide channels.";

            await Tools.Reply(e, show);
            
        };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
