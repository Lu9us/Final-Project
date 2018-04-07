using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Runtime
{
   public class AgentCreationStatement:Function
    {
       
        public string countVar;
        public string breed;

        public AgentCreationStatement(string[] body, int offset, string name) : base(body, offset, name)
        {
           
        }
    }
}
