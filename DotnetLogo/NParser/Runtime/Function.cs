using NParser.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Runtime
{
   public class Function
    {

        internal int pcOffset;
        internal string[] body;
        public  string name;
        public bool Report;
        public List<string> paramaters = new List<string>();
        public List<FlowControll> flowControls = new List<FlowControll>();
        public Function( string [] body,int offset,string name)
        {
            this.body = body;
            pcOffset = offset;
            this.name = name;
        }
    }

}
