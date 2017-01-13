using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using qtbot.CommandPlugin;
using Discord;
using qtbot.CommandPlugin.Attributes;

namespace qtbot.Modules
{
    class Information
    {
        [Command("commands"), Hidden(), 
            Description("Show this command list.")]
        public static async Task UserCommands(CommandArgs e)
        {
            string response = "The character to use a command right now is '/'.\n";
            foreach (var cmd in Bot._commands.Commands)
            {
                if (cmd.commandType != CommandType.User)
                    continue;

                if (!String.IsNullOrWhiteSpace(cmd.Purpose))
                {
                    response += $"**{cmd.Parts[0]}** - {cmd.Purpose}";

                    if (cmd.CommandDelay == null)
                        response += "\n";
                    else
                        response += $" **|** Time limit: once per {cmd.CommandDelayNotify} {cmd.timeType}.\n";
                }
            }

            var cnl = await e.Author.CreateDMChannelAsync();
            await cnl.SendMessageAsync(response);
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
                "Hi {0}. I used to make mean remarks when someone joined. I promised I changed but it's still hard. You mind if I do it one more time?",
                "I've never felt so good in my life! {0} has joined!!"
            };

            return reply;
        }

        public static async Task WelcomeUserAsync (Discord.WebSocket.DiscordSocketClient client, Discord.WebSocket.SocketUser u, ulong ServerID)
        {
            //Welcoming people.
            string[] replies = GetWelcomeReplies();
            string reply = String.Format(replies[BotTools.Tools.random.Next(replies.Length)], u.Mention);
            ITextChannel channelToAnswerIn = client.GetChannel(BotTools.Tools.GetServerInfo(ServerID).welcomingChannel) as ITextChannel;

            try
            {
                if (channelToAnswerIn != null)
                    await channelToAnswerIn.SendMessageAsync(reply);
            }
            catch (Exception ex)
            {
                BotTools.Tools.LogError("Couldn't send message.", ex.Message);
            }
        }       
    }
}
