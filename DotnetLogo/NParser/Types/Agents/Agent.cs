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
        }
    }
}
