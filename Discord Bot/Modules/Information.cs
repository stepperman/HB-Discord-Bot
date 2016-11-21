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
            //Only actually welcome people in the Hummingbird Discord server.
            if (server.Id != 99333280020566016)
                return;

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
                "Someone new? Awesome! Welcome {0}!",
                "Everyone, welcome {0}! They're new here!",
                "Oh, how nice, someone new! Hello {0}!",
                "My god! A new person! Hooray! Welcome, {0}.",
                "{0} doesn't even deserve a welcome. They deserve a party.",
                "🐸 OH SHIT IT {0} WHADDUP!",
                "Hi {0}, and welcome to the Weeaboo chat.",
                "Everyone welcome {0}, they're better at sex than anyone, now all they needs is a partner. You up for it?",
                "{0}, 👉 👌 😉",
                "{0}, 🍆 👌🏿 😫",
                "Hi {0}! Have I seen you somewhere around before? Probably not, I haven't seen someone so beautiful in quite some time!",
                "I'm excited!! Everyone, someone new just joined! It's {0}! They seem nice!",
                "I hope you'll enjoy your stay here, {0}. I'll try to improve it.",
                "Good day {0}! Or night. I don't know timezones, enjoy your stay.",
                "I think I'm in love with {0}, actually no. I'm pre-programmed to say this, sorry ;(.",
                "Hi {0}. I used to make mean remarks when someone joined. I promised I changed but it's still hard. You mind if I do it one more time?"
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
        }       
    }
}
