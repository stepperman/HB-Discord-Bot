using System;
using System.Collections.Generic;
using System.Text;

namespace qtbot.CommandPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    class DescriptionAttribute : Attribute
    {
        public string Description { get; }

        public DescriptionAttribute(string Description)
        {
            this.Description = Description;
        }
    }
}
