using NParser.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types.Internals
{
   public class OperatorFunctions
    {
        private static SystemState sys = SystemState.internalState;
        public static NetLogoObject let(NetLogoObject o, NetLogoObject n)
        {
          //  var v = sys.Assign(n.value.ToString());
            sys.exeStack.Peek().locals.Add((string)o.value, n);
            return new NetLogoObject() { ptrID = o.ptrID };
        }
        public static NetLogoObject report(NetLogoObject o, NetLogoObject d)
        {
            sys.exeStack.Peek().ReportValue = o;
            sys.BreakExecution = true;
            return new NetLogoObject() { ptrID = o.value.ToString() };

        }
        public static NetLogoObject show(NetLogoObject o, NetLogoObject d)
        {
            Console.WriteLine(o);
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }
        public static NetLogoObject set(NetLogoObject o, NetLogoObject n)
        {
            sys.exeStack.Peek().locals[(string)o.value] = n;
            return new NetLogoObject() { ptrID = o.ptrID };

        }
        public static Number add(Number n, Number b)
        {
            return new Number() {val = n.val + b.val };
        }
        public static Number divide(Number n, Number b)
        {
            return new Number() { val = n.val / b.val };
        }
        public static Number sub(Number n, Number b)
        {
            return new Number() { val = n.val - b.val };
        }
        public static Number order(Number n, Number b)
        {
            return new Number() { val = n.val * b.val };
        }
        public static NetLogoObject reset(NetLogoObject o, NetLogoObject n)
        {
            sys.globals["ticks"].value = 0;
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }

    }
}
