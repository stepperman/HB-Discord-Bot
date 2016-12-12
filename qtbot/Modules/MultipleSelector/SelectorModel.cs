using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace qtbot.Modules.MultipleSelector
{
    class MultiSelector<T> : MultiSelector
    {
        //TODO: Allow the page to have multiple pages...
        //Probably do this by setting a limit of 10, then if theres more it shows an extra dialog
        //saying that you can switch page with "n" or "p" (next, previous)
        //use a 2 Dimensional list/array for that.
        //No need for extra parameters, maybe a pages boolean not needed, just limit the array to 10
        //When creating a selector.
        public static MultiSelector<T> Create(List<List<T>> PossibleReplyValues, IGuildUser Creator)
        {
            MultiSelector<T> x = new MultiSelector<T>()
            {
                PossibleReplyValues = PossibleReplyValues,
                Creator = Creator,
                messagesToDelete = new List<IMessage>(),
                actionToPerform = (z) =>
                {
                    byte o;
                    bool parsed = byte.TryParse(z, out o);

                    if (o <= 0 || o > PossibleReplyValues.Count || !parsed)
                        return default(T);

                    return PossibleReplyValues[o - 1];
                }
            };
            return x;
        }


        public List<List<T>> PossibleReplyValues;
        public IGuildUser Creator;
        private Func<string, object> actionToPerform;

        public override IGuildUser GetUser()
        {
            return Creator;
        }

        public override Func<string, object> ReturnAction()
        {
            return actionToPerform;
        }

        public override void AddDeleteMessage(IMessage msg)
        {
            messagesToDelete.Add(msg);
        }

        public override Func<IMessage, IGuildUser, object, Task> GetResponse()
        {
            return respondAction;
        }

        public override void SetResponse(Func<IMessage, IGuildUser, object, Task> respondAction)
        {
            this.respondAction = respondAction;
        }

    }

    public abstract class MultiSelector
    {
        public abstract IGuildUser GetUser();
        public abstract Func<string, object> ReturnAction();
        public abstract void AddDeleteMessage(IMessage msg);
        public abstract Func<IMessage, IGuildUser, object, Task> GetResponse();
        public abstract void SetResponse(Func<IMessage, IGuildUser, object, Task> a);

        public Func<IMessage, IGuildUser, object, Task> respondAction;
        public List<IMessage> messagesToDelete;
        public bool canRespond = false;
        public byte neededPages = 1;
        public byte currentPage = 1;
    }
}
