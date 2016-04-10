using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Discord_Bot.CommandPlugin;
using Discord;

namespace Discord_Bot
{
    class Information
    {
        public static Func<CommandArgs, Task> Commands = async e =>
        {
            string response = $"The character to use a command right now is '{Program._commands.CommandChar}'.\n";
            foreach (var cmd in Program._commands._commands)
            {
                if (!String.IsNullOrWhiteSpace(cmd.Purpose))
                {
                    response += $"**{cmd.Parts[0]}** - {cmd.Purpose}";

                    if (cmd.CommandDelay == null)
                        response += "\n";
                    else
                        response += $" **|** Time limit: once per {cmd.CommandDelayNotify} {cmd.timeType}.\n";
                }
            }

            await e.User.SendMessage(response);
        };

        public static async Task NewUserText(User e, Server server)
        {
            try
            {
                StreamReader streamReader = new StreamReader("../BeginnersText.txt");

                string GettingStarted = await streamReader.ReadToEndAsync();
                GettingStarted = String.Format(GettingStarted, e.Mention, server.GetUser(83677331951976448).Mention);

                streamReader.Close();

                string[] reply = GettingStarted.Split(new string[] { "[SPLIT]" }, StringSplitOptions.None);

                foreach (var message in reply)
                    await e.SendMessage(message);
            }
            catch (Exception ex)
            {
                Tools.LogError("Couldn't send new user text!", ex.Message);
            }
        }
        
        public static async Task WelcomeUser (DiscordClient client, User u, ulong ServerID)
        {
            //Welcoming people.
            string[] replies = { "Holy shit, a new user! Welcome {0}!", "Ew, a newfag! Get out {0}!", "Someone new? Awesome! Welcome {0}!", "Everyone, welcome {0}! He's new here!", "Oh, how nice, someone new! Hello {0}!", "My god! A new person! We don't need you, just leave {0}.", "{0}, it might be better if you'd leave. You'd just be a newfag. Well, welcome anyway!" };
            string reply = String.Format(replies[Tools.random.Next(replies.Length)], u.Mention);
            Channel channelToAnswerIn =  client.GetChannel(Tools.GetServerInfo(ServerID).welcomingChannel);

            try
            {
                if (channelToAnswerIn != null)
                    await channelToAnswerIn.SendMessage(reply);
            }
            catch (Exception ex)
            {
                Tools.LogError("Couldn't send message.", ex.Message);
            }

            //Adding user's role upon joining the server.
            var role = Tools.GetServerInfo(ServerID).standardRole;
            if (role != null)
            {
                try
                {
                    await u.Edit(null, null, u.VoiceChannel, new Role[] { client.GetServer(ServerID).GetRole(ulong.Parse(role)) });
                }
                catch (Exception) { }
            }
        }

        public static Func<CommandArgs, Task> SeeMentions = async e =>
        {
            List<Storage.msg> list = null;
            if (Storage.UserMentions.TryGetValue(e.User.Id, out list))
            {
                string message = "";
                foreach (var msg in list)
                {
                    message += $"**{msg.Author}**: {msg.Message}\n";
                }

                message = message.Substring(0, 1999);
                await e.User.SendMessage(message);


                Storage.UserMentions.Remove(e.User.Id);
            }
        };

        public static void OfflineMessage(MessageEventArgs e)
        {
            if (e.User.Id == Storage.client.CurrentUser.Id)
                return;

            List<Storage.msg> list = null;
            if (!Storage.UserMentions.TryGetValue(e.User.Id, out list))
            {
                list = new List<Storage.msg>();
                Storage.UserMentions.Add(e.User.Id, list);
            }

            list.Add(new Storage.msg() { Author = e.User.Name, Message = e.Message.Text });
            if (list.Count == 16)
                list.RemoveAt(0);
        }
    }
}
