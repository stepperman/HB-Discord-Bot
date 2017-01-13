using System;
using System.Threading.Tasks;
using qtbot.CommandPlugin.Attributes;

namespace qtbot.CommandPlugin
{
    public sealed class Command
    {
        public string Text { get; }
        internal string[] alias;
        public int? MinArgs { get; internal set; }
        public int? MaxArgs { get; internal set; }
        public string Purpose { get; internal set; }
        public int MinPerms { get; internal set; }
        public bool IsHidden { get; internal set; }
        public bool DelayUnignorable { get; internal set; }
        public string timeType { get; internal set; }
        public int? CommandDelayNotify { get; internal set; }
        public int? CommandDelay { get; internal set; }
        public Permission permission { get; internal set; }
        internal readonly string[] Parts;
        internal Func<CommandArgs, Task> Handler;
        internal Func<CommandArgs, Task> FailHandler;

        public Command(string text)
        {
            Text = text;
            Parts = text.ToLowerInvariant().Split(' ');
        }

        internal void SetAliases(string[] alias)
        {
            this.alias = alias;
        }

    }
}
