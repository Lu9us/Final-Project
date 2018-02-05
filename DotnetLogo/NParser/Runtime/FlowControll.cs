using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Runtime
{
    public class FlowControll
    {
      public enum JumpType
        {
            loopStart,
            Succes,
            Fail

        }

        public struct Block
        {
            public int start;
            public int end;

        }

        internal string type;
        public string conditionalLine;
        public Dictionary<JumpType,Block > JumpTable = new Dictionary<JumpType, Block>();
        public FlowControll(string type)
        {
            this.type = type;

        }
    }
}
