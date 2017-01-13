using System;
using System.Collections.Generic;
using System.Text;

namespace qtbot.CommandPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    class ArgsAttribute : Attribute
    {
        public ArgsType argType { get; }
        public int First { get; }
        public int Last { get; }

        public ArgsAttribute(ArgsType type, int first = 1, int last = 999)
        {
            argType = type;
            First = first;
            Last = last;
        }
    }

    public enum ArgsType
    {
        ArgsAtLeast,
        ArgsAtMax,
        ArgsBetween,
        ArgsNone
    }
}
