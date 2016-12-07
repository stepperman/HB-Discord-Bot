using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.Linq;

namespace qtbot.Modules.MultipleSelector
{
    class MultiSelectorController
    {
        public static List<MultiSelector> selectors = new List<MultiSelector>();

        public static async Task ReceivedMessageAsync(Discord.IMessage message)
        {
            MultiSelector selector = selectors.FirstOrDefault(x => x.GetUser().Id == message.Author.Id);
            if (selector == null)
                return;

            selector.ReturnAction()(message);
            selectors.Remove(selector);
        }

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
        public static async Task CreateSelector<T>(IMessage msg, T[] t, Func<IMessage, object> actionToPerform = null)
        {
            if (selectors.Count != 0)
            {
                //If the user is already in the list, don't do anything for him thank you.
                if (WaitingOnUser(msg.Author as IGuildUser))
                    return;
            }

            if (actionToPerform == null)
                selectors.Add(MultiSelector<T>.Create(t, (msg.Author as IGuildUser)));
            else
                selectors.Add(MultiSelector<T>.Create(t, (msg.Author as IGuildUser), actionToPerform));

            string reply = "Please select:\n```";
            for(int i = 0; i < t.Length; i++)
            {
                reply += "#" + (i+1) + " " + t.ToString() + "\n";
            }
            reply += "```";

            await msg.Channel.SendMessageAsync(reply);
        }
    }

    class MultiSelector<T> : MultiSelector
    {
        public static MultiSelector<T> Create(T[] PossibleReplyValues, IGuildUser Creator)
        {
            MultiSelector<T> x = new MultiSelector<T>()
            {
                PossibleReplyValues = PossibleReplyValues,
                Creator = Creator,
                messagesToDelete = new List<IMessage>(),
                actionToPerform = (z) =>
                {
                    uint o;
                    bool parsed = uint.TryParse(z.Content, out o);

                    if (o < 0 || o >= PossibleReplyValues.Length || !parsed)
                        return default(T);

                    return PossibleReplyValues[o-1];
                }
            };
            return x;
        }

        public static MultiSelector<T> Create(T[] PossibleReplyValues, IGuildUser Creator, Func<IMessage, object> actionToPerform)
        {
            var x = new MultiSelector<T>()
            {
                PossibleReplyValues = PossibleReplyValues,
                Creator = Creator,
                messagesToDelete = new List<IMessage>(),
                actionToPerform = actionToPerform
            };
            return x;
        }


        public T[] PossibleReplyValues;
        public List<IMessage> messagesToDelete;
        public IGuildUser Creator;
        private Func<IMessage, object> actionToPerform;

        public override IGuildUser GetUser()
        {
            return Creator;
        }

        public override Func<IMessage, object> ReturnAction()
        {
            return actionToPerform;
        }

        public override void AddDeleteMessage(IMessage msg)
        {
            messagesToDelete.Add(msg);
        }

    }

    public abstract class MultiSelector
    {
        public abstract IGuildUser GetUser();
        public abstract Func<IMessage, object> ReturnAction();
        public abstract void AddDeleteMessage(IMessage msg);
    }
}
