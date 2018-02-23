using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types.Agents
{
   public class AgentSet: NetLogoObject
    {
        public AgentSet(List<MetaAgent> data)
        {
            value = data;
       
        }
        private List<MetaAgent> val;
        public override object value { get { return val; } set { val = (List<MetaAgent>)value; } }
    }
}
