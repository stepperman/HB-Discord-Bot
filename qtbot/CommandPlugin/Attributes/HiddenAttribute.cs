using System;
using System.Collections.Generic;
using System.Text;

namespace qtbot.CommandPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    class HiddenAttribute : Attribute
    {
        public HiddenAttribute() { }
    }
}
