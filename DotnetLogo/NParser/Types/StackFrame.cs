using NParser.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
   public class StackFrame
    {
       internal StackFrame(string name,Dictionary<string,NetLogoObject> param,Function bf)
        {
            FunctionName = name;
            this.param = param;
            baseFunction = bf;
        }
        public readonly string FunctionName;
        internal  Dictionary<string, NetLogoObject> param = new Dictionary<string, NetLogoObject>();
        internal Dictionary<string, NetLogoObject> locals = new Dictionary<string, NetLogoObject>();
        internal bool Report;
        internal NetLogoObject ReportValue;
        internal bool isAsk;
        internal bool flowControl;
        internal int pc;
        internal bool anonymousFunction = false;
        internal Function baseFunction;
      
        public override string ToString()
        {
            String s = "[";
            s += FunctionName + Environment.NewLine;
            s += "Paramaters [";
            s +=  Environment.NewLine;
            foreach (KeyValuePair<string, NetLogoObject> par in param)
            {
                s += par.Key + "    " + par.ToString();
                s += Environment.NewLine;
            }
            s +=  Environment.NewLine;
            s += "]";
            foreach (KeyValuePair<string, NetLogoObject> par in locals)
            {
                s += par.Key + "    " + par.Value.ToString();
                s += Environment.NewLine;
            }
            s += "program counter: " + pc;
            s += Environment.NewLine;
            s += "report: " + Report;
            s += Environment.NewLine;
            s += "report val: " + ReportValue;
            s += Environment.NewLine;
            s += "is ask: " + isAsk;
            s+= "]";
            return s.ToString();
        }
    }
}
