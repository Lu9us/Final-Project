using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
   public class MetaAgent: AgentProperties,NetLogoObject
    {
        
        public int x, y;
        public int ID;
        protected MetaAgent()
        {
            AddProperty("X", new Integer());
            AddProperty("Y", new Integer());
            protectedType.Add("X", typeof(Integer));
            protectedType.Add("Y", typeof(Integer));
           
        }

    }
}
