using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Discord_Bot
{
    class ChatEventActions
    {
        //When a user joins the server it welcomes the user.
        public static async void UserJoined(object sender, UserEventArgs e)
        {
            await Information.WelcomeUser(Storage.client, e.User, e.Server.Id);
        }

        //When a user leaves, say goodbye.
        public static async void UserLeft(object sender, UserEventArgs e)
        {
            var server = Tools.GetServerInfo(e.Server.Id);

            if (server.welcomingChannel == 0)
                return;

            await Tools.Reply(e.User, Storage.client.GetChannel(server.welcomingChannel), 
                $"Goodbye, **{e.User.Mention}**. It was nice having you here. ({e.User.Name})", 
                false);
        }

        //When a user has been banned, tell it them in the welcomings channel.
        public static async void UserBanned(object sender, UserEventArgs e)
        {
            var server = Tools.GetServerInfo(e.Server.Id);

            if (server.welcomingChannel == 0)
                return;

            await Tools.Reply(e.User, Storage.client.GetChannel(server.welcomingChannel), 
                $"**{e.User.Mention}** has been banned from the server. ({e.User.Name})", 
                false);
        }

        /// <summary>
        /// When a message has been received, call a couple functions which
        /// controls the ayy channel, the YouTube command which needs to be global. 
        /// (So that I can easily add it wiht multiple commands)
        /// And it controls the regular users so that messages get added and blahblah.
        /// </summary>
        public static async void MessageReceived(object sender, MessageEventArgs e)
        {
            await Modules.Games.AyyGame.Game(e);
            await Modules.YouTube.ReceivedMessage(e.Message);

            if (e.Channel.IsPrivate)
                return;

            if (Tools.GetServerInfo(e.Server.Id).RegularUsersEnabled)
                await RegularUsers.ReceivedMessage(e);
        }
        
        // If the bot has been disconnected, try to reconnect.
        public static async void Disconnected(object sender, DisconnectedEventArgs e)
        {
            while (Storage.client.State != ConnectionState.Connected)
            {
                try
                {
                    await Storage.client.Connect(Storage.programInfo.username, Storage.programInfo.password);
                }
                catch (Exception ex)
                {
                    Tools.LogError("Couldn't connect!", ex.Message);
                }

                await Task.Delay(30000);
            }
        }

    }
}
