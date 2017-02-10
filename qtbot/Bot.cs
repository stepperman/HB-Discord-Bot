using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using qtbot.CommandPlugin;

namespace qtbot
{
    class Bot
    {
        public static DiscordSocketClient Client { get; private set; }
        public static CommandsPlugin _commands;

        public async Task StartAsync()
        {
            var client = new DiscordSocketClient();
            Bot.Client = client;
            Bot.Client.Log += Client_Log;

            _commands = new CommandsPlugin(Client, (s, a) => BotTools.Tools.GetPerms(a, s as IGuildUser));

            BotTools.Setup.GetProgramInfo();

            Bot.Client.UserJoined += BotTools.ChatEventActions.UserJoinedAsync;
            Bot.Client.UserBanned += BotTools.ChatEventActions.UserBannedAsync;
            Bot.Client.MessageReceived += BotTools.ChatEventActions.MessageReceivedAsync;
            Bot.Client.Disconnected += BotTools.ChatEventActions.DisconnectedAsync;


            _commands.CommandError += async (s, e) =>
            {
                if (e.Command.Text == "ayy")
                    return;

                var ex = e.Exception;
                if (ex is PermissionException)
                    await BotTools.Tools.ReplyAsync(e, "Sorry, you do not have the permissions to use this command!");
                else
                    await BotTools.Tools.ReplyAsync(e, $"Error: {ex.Message}.");

            };
            
            await Modules.AnimeTools.AuthorizeAnilistAsync();
            await LoginAsync();

            await Task.Delay(-1);

        }

        public async Task LoginAsync()
        {
            try
            {
                await Client.LoginAsync(TokenType.Bot, (string)BotTools.Storage.programInfo.bot_token);
                await Client.ConnectAsync();
                BotTools.Storage.client = Client;
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }

        private Task Client_Log(LogMessage arg)
        {
            Console.WriteLine($"[{arg.Severity}] {arg.Source}: {arg.Message}.");
            var f = System.IO.File.CreateText("errors.txt");
            f.WriteLine(arg.Exception?.ToString() + "\n\n");
            f.Dispose();
            return Task.CompletedTask;
        }
    }
}
