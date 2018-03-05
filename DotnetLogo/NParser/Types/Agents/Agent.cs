using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types.Agents
{
    public class Agent: MetaAgent
    {
        private static int IDCOUNT = 0;

        public Agent()
        {
            ID = IDCOUNT;
            IDCOUNT++;
            properties.AddProperty("rotation", new Number() { value = 0 });
            properties.AddProperty("color", new NSString());
            properties.protectedType.Add("color", typeof(NSString));
            properties.protectedType.Add("rotation", typeof(Number));
            properties.properties["color"] = new NSString() { val = "black" };
            value = "Agent";
        }
        public override string ToString()
        {
            return "Agent " + ID;
        }
    }
}
