using System;
using System.Runtime.CompilerServices;

namespace TqLib.ccnet.Core
{
    public class PluginArgumentAttribute : Attribute
    {
        public string Name { get; private set; }
        public bool Require { get; set; } = true;

        public PluginArgumentAttribute([CallerMemberName] string name = null)
        {
            Name = name;
        }
    }
}