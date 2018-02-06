using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
   public class StackFrame
    {
       internal StackFrame(string name,Dictionary<string,NetLogoObject> param)
        {
            FunctionName = name;
            this.param = param;
        }
        public readonly string FunctionName;
        public readonly  Dictionary<string, NetLogoObject> param = new Dictionary<string, NetLogoObject>();
        internal Dictionary<string, NetLogoObject> locals = new Dictionary<string, NetLogoObject>();
        internal bool Report;
        internal NetLogoObject ReportValue;
        internal bool isAsk;
    }
}
