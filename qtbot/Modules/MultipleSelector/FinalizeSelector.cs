using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;

namespace qtbot.Modules.MultipleSelector
{
    partial class MultiSelectorController
    {
        //React with text
        public static async Task ReceivedMessageAsync(Discord.IMessage message)
        {
            MultiSelector selector = selectors.FirstOrDefault(x => x.GetUser().Id == message.Author.Id);
            if (selector == null)
                return;

            switch(message.Content.ToLower())
            {
                case "n":
                    NextPage();
                    selector.AddDeleteMessage(message);
                    return;
                case "p":
                    PreviousPage();
                    selector.AddDeleteMessage(message);
                    return;
            }

            selector.AddDeleteMessage(message);
            var obj = selector.ReturnAction()(message.Content);
            await selector.GetResponse()(message, message.Author as IGuildUser, obj);
            selectors.Remove(selector); //Delete if completed successfully

            foreach (var msg in selector.messagesToDelete)
            {
                try
                {
                    await msg.DeleteAsync();
                }
                catch (Exception) { }
            }
        }
    }
}
