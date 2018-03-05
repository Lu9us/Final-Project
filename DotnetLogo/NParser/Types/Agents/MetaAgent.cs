using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types.Agents
{
   public class MetaAgent:NetLogoObject
    {
        public AgentProperties properties;
        public int x, y;
        public int ID;
        protected MetaAgent()
        {
            properties = new AgentProperties();
            properties.AddProperty("X", new Integer());
            properties.AddProperty("Y", new Integer());
            properties.protectedType.Add("X", typeof(Integer));
            properties.protectedType.Add("Y", typeof(Integer));
            value = "MetaAgent";
        }
        public override string ToString()
        {
            return "Agent "+ID;
        }
    }
}
