using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.Linq;

namespace qtbot.Modules.MultipleSelector
{
    //TODO: Allow the page to have multiple pages...
    //Probably do this by setting a limit of 10, then if theres more it shows an extra dialog
    //saying that you can switch page with "n" or "p" (next, previous)
    //use a 2 Dimensional list/array for that.
    //No need for extra parameters, maybe a pages boolean not needed, just limit the array to 10
    //When creating a selector.
    partial class MultiSelectorController
    {
        public static List<MultiSelector> selectors = new List<MultiSelector>();
        
        private static bool WaitingOnUser(IGuildUser Author)
        {
            for (int i = 0; i < selectors.Count; i++)
            {
                if ((selectors[i].GetUser().Id == Author.Id))
                    return true;
            }

            return false;
        }

        private static MultiSelector FindUser(IGuildUser user)
        {
            foreach(var c in selectors)
            {
                if (c.GetUser().Id == user.Id)
                    return c;
            }

            return null;
        }
        
        /// <summary>
        /// Creates a selector, this does not have a limit but it's probably best practice to actually to add a limit. Do what you want.
        /// </summary>
        /// <typeparam name="T">The type the selector is.</typeparam>
        /// <param name="msg">The message that was contained when the selector was created.</param>
        /// <param name="t">An array with possible options.</param>
        /// <param name="actionToPerform">The function to perform that returns the correct object</param>
        public static async Task<MultiSelector<T>> CreateSelectorAsync<T>(IMessage msg, T[] t, Func<IMessage, object> actionToPerform = null)
        {
            if (selectors.Count != 0)
            {
                //If the user is already in the list, don't do anything for him thank you.
                if (WaitingOnUser(msg.Author as IGuildUser))
                    return null;
            }

            var x = MultiSelector<T>.Create(t, (msg.Author as IGuildUser));
            selectors.Add(x);

            string reply = "Please select:\n```";
            for(int i = 0; i < 10; i++)
            {
                if (t.Length == i)
                    break;
                reply += "#" + (i+1) + " " + t[i].ToString() + "\n";
            }
            reply += "```";

            var replyMsg = await msg.Channel.SendMessageAsync(reply);
            x.AddDeleteMessage(replyMsg);

            return x;
        }
    }
}
