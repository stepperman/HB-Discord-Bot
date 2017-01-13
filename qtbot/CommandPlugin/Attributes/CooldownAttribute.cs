using System;
using System.Collections.Generic;
using System.Text;

namespace qtbot.CommandPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    class CooldownAttribute : Attribute
    {
        public Cooldowns Cooldown { get; }
        public int Time { get; }

        public CooldownAttribute(int time, Cooldowns cooldown)
        {
            Time = time;
            Cooldown = cooldown;
        }
    }

    public enum Cooldowns
    {
        Seconds,
        Minutes,
        Hours
    }
}
