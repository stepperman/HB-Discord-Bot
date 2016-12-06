using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace qtbot.BotTools
{
    class ChatEventActions
    {
        //When a user joins the server it welcomes the user.
        public static async Task UserJoinedAsync(SocketGuildUser e)
        {
            await Modules.Information.WelcomeUserAsync(Storage.client, e, e.Guild.Id);
        }

        //When a user leaves, say goodbye.
        public static async Task UserLeftAsync(SocketGuildUser e)
        {
            var server = Tools.GetServerInfo(e.Guild.Id);

            if (server.welcomingChannel == 0)
                return;

            await Tools.ReplyAsync(e, Storage.client.GetChannel(server.welcomingChannel) as ITextChannel, 
                $"Goodbye, **{e.Mention}**. It was nice having you here. ({e.Username})", 
                false);
        }

        //When a user has been banned, tell it them in the welcomings channel.
        public static async Task UserBannedAsync(SocketUser user, SocketGuild guild)
        {
            var server = Tools.GetServerInfo(guild.Id);

            if (server.welcomingChannel == 0)
                return;

            await Tools.ReplyAsync(user, Storage.client.GetChannel(server.welcomingChannel) as ITextChannel, 
                $"**{user.Mention}** has been banned from the server. ({user.Username})", 
                false);
        }

        /// <summary>
        /// When a message has been received, call a couple functions which
        /// controls the ayy channel, the YouTube command which needs to be global. 
        /// (So that I can easily add it wiht multiple commands)
        /// And it controls the regular users so that messages get added and blahblah.
        /// </summary>
        public static async Task MessageReceivedAsync(SocketMessage message)
        {
            await Modules.Games.AyyGame.GameAsync(message);
            await Modules.YouTube.ReceivedMessageAsync(message);

            if ((message.Channel as ITextChannel) != null)
                return;

            if (Tools.GetServerInfo((message.Channel as IGuildChannel).Guild.Id).RegularUsersEnabled)
                await Modules.RegularUsers.ReceivedMessageAsync(message);
        }
        
        // If the bot has been disconnected, try to reconnect.
        public static async Task DisconnectedAsync(Exception ex)
        {
            while (Storage.client.ConnectionState != ConnectionState.Connected)
            {
                try
                {
                    await Storage.client.LoginAsync(TokenType.Bot, Storage.programInfo.token);
                }
                catch (Exception exe)
                {
                    Tools.LogError("Couldn't connect!", ex.Message);
                }

                await Task.Delay(30000);
            }
        }

    }
}
