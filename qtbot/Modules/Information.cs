using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using qtbot.CommandPlugin;
using Discord;
using qtbot.CommandPlugin.Attributes;
using Discord.WebSocket;

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
                    response += $"**{string.Join(" ", cmd.Parts)}** - {cmd.Purpose}";

                    if (cmd.CommandDelay == null)
                        response += "\n";
                    else
                        response += $" **|** Time limit: once per {cmd.CommandDelayNotify} {cmd.timeType}.\n";
                }
            }

            var cnl = await e.Author.CreateDMChannelAsync();
            await cnl.SendMessageAsync(response);
        }

        [Command("source"),
            Description("Show the bot's source code.")]
        public static async Task SourceCode(CommandArgs e) =>
            await BotTools.Tools.ReplyAsync(e, "Here is my source code: <https://github.com/stepperman/qtbot>");
        
        [Command("lmao")]
        public static async Task Lmao(CommandArgs e) =>
            await BotTools.Tools.ReplyAsync(e, "https://www.youtube.com/watch?v=HTLZjhHIEdw");

        [Command("no")]
        public static async Task No(CommandArgs e) =>
            await BotTools.Tools.ReplyAsync(e, "pignig", false);

        [Command("togglensfw"),
            Description("Toggle NSFW on/off")]
        public static async Task CmdToggleNSFW(CommandArgs e)
        {
            var channel = e.Guild.Channels.FirstOrDefault(x => 
                string.Equals(x.Name, "nsfw", StringComparison.OrdinalIgnoreCase));

            if (channel == null)
                return;

            bool hidden = channel.GetPermissionOverwrite(e.Author) == null;

            if(!hidden)
                await channel.RemovePermissionOverwriteAsync(e.Author);
            else
                await channel.AddPermissionOverwriteAsync(e.Author, 
                    new OverwritePermissions(readMessages: PermValue.Allow));

            string msg = !hidden ? "has now been hidden from you." : "now shows up for you.";
            await BotTools.Tools.ReplyAsync(e, $"#{channel.Name} {msg}");
        }

        [Command("donate", CommandType.User, "patreon", "donation"),
            Description("show the link of patreon page.")]
        public static async Task CmdDonateMessage(CommandArgs e) =>
                await e.ReplyAsync("Would you be so kind to donate? It'll mean a lot to me 💖. https://www.patreon.com/qtbot");

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
                "Everyone welcome {0}, they're better at sex than anyone, now all they needs is a partner. You up for it?",
                "{0}, 👉 👌 😉",
                "I'm excited!! Everyone, someone new just joined! It's {0}! They seem nice!",
                "hey {0} welcome to the server now donate to my patreon or fuck off",
                "special",
                "I'm not even going to welcome {0}"
            };

            return reply;
        }

        public static async Task PerformSpecialWelcome(IUser user, ITextChannel channel)
        {
            var typing = channel.EnterTypingState();
            await Task.Delay(TimeSpan.FromSeconds(0.4));
            typing.Dispose();
            await channel.SendMessageAsync("wow");

            typing = channel.EnterTypingState();
            await Task.Delay(TimeSpan.FromSeconds(1.4));
            typing.Dispose();
            await channel.SendMessageAsync($"welcome, {user.Mention}");

            typing = channel.EnterTypingState();
            await Task.Delay(TimeSpan.FromSeconds(2));
            typing.Dispose();
            await channel.SendMessageAsync("you've managed to find this place");

            typing = channel.EnterTypingState();
            await Task.Delay(TimeSpan.FromSeconds(0.4));
            typing.Dispose();
            await channel.SendMessageAsync("its...");

            typing = channel.EnterTypingState();
            await Task.Delay(TimeSpan.FromSeconds(3));
            typing.Dispose();
            await channel.SendMessageAsync("eh");
        }

        [Command("specialwelcome", CommandType.Admin)]
        public static async Task CmdSpecialWelcome(CommandArgs e)
            => await PerformSpecialWelcome(e.Message.MentionedUsers.ToList()[0], e.Channel);

        public static async Task WelcomeUserAsync (Discord.WebSocket.DiscordSocketClient client, Discord.WebSocket.SocketUser u, ulong ServerID)
        {
            //Welcoming people.
            string[] replies = GetWelcomeReplies();
            string _tempreply = replies[RandomNumber.Next(replies.Length)];

            ITextChannel channelToAnswerIn = client.GetChannel(BotTools.Tools.GetServerInfo(ServerID).welcomingChannel) as ITextChannel;

            if (_tempreply.Equals("special"))
            {
                await PerformSpecialWelcome(u, channelToAnswerIn);
                return;
            }

            string reply = String.Format(_tempreply, u.Mention);
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
