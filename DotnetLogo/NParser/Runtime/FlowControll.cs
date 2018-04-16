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
     

        public class Block: Function
        {
            public int start;
            public int end;

            public Block(int start,string [] s,JumpType jt): base(s,start,jt.ToString())
            {
                this.start = start;
                this.end = end;
            }
        }

        internal int GetTotalJump()
        {
            int i = 0;
            foreach (Block b in JumpTable.Values)
            {
                i += b.body.Length;
            }
            return i + ((2*JumpTable.Count) - 2 );

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
