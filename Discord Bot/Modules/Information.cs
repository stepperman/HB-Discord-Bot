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

        public static string[] GetWelcomeReplies()
        {
            string[] reply =  {
                "Holy shit, a new user! Welcome {0}!",
                "Ew, a newfag! Get out {0}!",
                "Someone new? Awesome! Welcome {0}! Just kidding, piss off.",
                "Everyone, welcome {0}! Or don't. I don't care.",
                "Oh, how nice, someone new! Hello {0}!",
                "My god! A new person! We don't need you, just leave {0}.",
                "{0}, it might be better if you'd leave. You'd just be a newfag. Well, welcome anyway!",
                "Wat de fuck?! {0}, rot eens op jij kanker hond!",
                "Disgusting! {0} smells, kick them out!",
                "I'm not even going to welcome {0}.",
                "WTF are you doing here, {0}?",
                "If I wanted a {0}, I'd have bought a dog.",
                "If I wanted a {0}, I'd have bought a gorilla.",
                "{0} doesn't even deserve a welcome. Hi, I guess...",
                "🐸 OH SHIT IT {0} WHADDUP!",
                "{0}... If I promise to miss you, will you go away?",
                "Hi {0} welcome to the hummingbird chat!! Where you'll be alone: In bad company.",
                "Everyone welcome {0}, they're better at sex than anyone, now all they needs is a partner.",
                "Welcome {0} Can I borrow your face for a few days while my ass is on vacation?.",
                "Hey {0}, why are you here! Did the mental hospital test too many drugs on you today?",
                "Welcome {0}, now we are depriving a village of an idiot.",
                "Hello {0}, I think I've seen you before today? I was at the zoo.",
                "Hello {0} You're very smart. You have brains you never used.", //THIS IS A COMPLIMENT. How is it a compliment, exactly?
                "Yuck! Get out {0}!! I've come across decomposed bodies that are less disgusting than you are!!",
                "You better go away {0}, before I start taunting you. Actually, you know what? Welcome.",
                "Welcome {0}, you will be utterly forgotten.",
                "Hey {0}!!!! ...What doesn’t kill you... disappoints me.",
                "Hi {0}, you deserve a hug right now.",
                "{0}, 👉 👌 😉",
                "{0}, 🍆 👌🏿 😫",
                "{0}! Ur nan's a cunt m8 and your step-dad Dave's a fookin' plonka!"
            };

            return reply;
        }

        public static async Task WelcomeUser (DiscordClient client, User u, ulong ServerID)
        {
            //Welcoming people.
            string[] replies = GetWelcomeReplies();
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

                if(message.Length > 1999)
                    message = message.Substring(0, 1999);
                await e.User.SendMessage(message);

                Storage.UserMentions.Remove(e.User.Id);
            }
        };

        public static void OfflineMessage(MessageEventArgs e)
        {
            if (e.Message.MentionedUsers.Count() == 0)
                return;

            if (e.User.Id == Storage.client.CurrentUser.Id)
                return;

            var users = e.Message.MentionedUsers.ToArray();

            foreach (var usr in users)
            {
                List<Storage.msg> list = null;
                if (!Storage.UserMentions.TryGetValue(usr.Id, out list))
                {
                    list = new List<Storage.msg>();
                    Storage.UserMentions.Add(usr.Id, list);
                }

                list.Add(new Storage.msg() { Author = e.User.Name, Message = e.Message.Text });
                if (list.Count == 20)
                    list.RemoveAt(0);
            }
        }
    }
}
