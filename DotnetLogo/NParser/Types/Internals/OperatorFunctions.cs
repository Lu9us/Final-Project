using NParser.Runtime;
using NParser.Types.Agents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NParser.Types.Internals
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OperatorName : Attribute
    {
        public string name { get; set; }
        public bool cast { get; set; } = true;
        public bool treatAsVal { get; set; } = false;
    }

    public static class OperatorFunctions
    {

        private static SystemState sys = SystemState.internalState;

        public static void ResetSystemState()
        {
            sys = SystemState.internalState;
        }
        [OperatorName(cast = false, name = "elseif")]
        [OperatorName(cast = false, name = "if")]
        public static Boolean IF(Boolean o, NetLogoObject n)
        {

            return o;

        }
        [OperatorName(cast = true, name = "=")]
        public static Boolean Equal(NetLogoObject o, NetLogoObject n)
        {
            if (!string.IsNullOrEmpty(o.ptrID))
            {
                o = sys.Assign(o.ptrID);
            }
            if (!string.IsNullOrEmpty(n.ptrID))
            {
                n = sys.Assign(n.ptrID);
            }
            if (o.value.Equals(n.value))
            {
                return new Boolean() { val = true };
            }
            else
            {
                return new Boolean() { val = false };
            }
        }
        [OperatorName(cast = true, name = "!=")]
        public static Boolean Not(NetLogoObject o, NetLogoObject n)
        {

            if (!string.IsNullOrEmpty(o.ptrID))
            {
                o = sys.Assign(o.ptrID);
            }
            if (!string.IsNullOrEmpty(n.ptrID))
            {
                n = sys.Assign(n.ptrID);
            }
            if (!o.value.Equals(n.value))
            {
                return new Boolean() { val = true };
            }
            else
            {
                return new Boolean() { val = false };
            }

        }

        [OperatorName(cast = true, name = ">")]
        public static Boolean Over(Number o, Number n)
        {
            if (o.val > n.val)
            {
                return new Boolean() { val = true };
            }
            else
            {
                return new Boolean() { val = false };
            }
        }
        [OperatorName(cast = true, name = "<")]
        public static Boolean Under(Number o, Number n)
        {
            if (o.val < n.val)
            {
                return new Boolean() { val = true };
            }
            else
            {
                return new Boolean() { val = false };
            }
        }

        [OperatorName(cast = true, name = "%")]
        public static Number Mod(Number n, Number o)
        {

            return new Number() { val = n.val % o.val };

        }

        [OperatorName(cast = false, name = "tick")]
        public static NetLogoObject Tick(NetLogoObject o, NetLogoObject n)
        {

            sys.globals["ticks"].value = ((float)sys.globals["ticks"].value) + 1;
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }

        [OperatorName(cast = false, name = "let")]
        public static NetLogoObject Let(NetLogoObject o, NetLogoObject n)
        {
            
            if (o.ptrID != null)
            {
                //  var v = sys.Assign(n.value.ToString());
                sys.GetCurrentFrame().locals.Add((string)o.value, n);
            }
            else
            {
                sys.GetCurrentFrame().locals.Add((string)n.value, o);
            }
            
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }

        [OperatorName(cast = false, name = "report")]
        public static NetLogoObject Report(NetLogoObject o, NetLogoObject d)
        {
            sys.GetCurrentFrame().ReportValue = o;
            sys.BreakExecution = true;
            return new NetLogoObject() { ptrID = o.value.ToString() };

        }
        [OperatorName(name = "get-n",cast = true)]
        public static NetLogoObject GetNeighbours(Number i, Number j)
        {
            List<MetaAgent> data = sys.GetNeighbours((int)i.val, (int)j.val);
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }
        [OperatorName(name = "show",cast = true)]
        public static NetLogoObject Show(NetLogoObject o, NetLogoObject d)
        {
            Console.WriteLine(o.value);
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }

        [OperatorName(cast = false, name = "set")]
        public static NetLogoObject Set(NetLogoObject o, NetLogoObject n)
        {
           
                if (o.ptrID != null)
                {
                    n = sys.Assign(n.value.ToString(), true);
                    sys.set((string)o.value, n);
                }
                else
                {
                    o = sys.Assign(o.value.ToString(), true);
                    sys.set((string)n.value, o);
                }


                return new NetLogoObject() { ptrID = "NULLPTR" };


        }
        [OperatorName(name = "+")]
        public static Number Add(Number n, Number b)
        {
            return new Number() { val = n.val + b.val };
        }
        [OperatorName(name = "/")]
        public static Number Divide(Number n, Number b)
        {
            return new Number() { val = n.val / b.val };
        }
        [OperatorName(name = "-")]
        public static Number Sub(Number n, Number b)
        {
            return new Number() { val = n.val - b.val };
        }
        [OperatorName(name = "*")]
        public static Number Order(Number n, Number b)
        {
            return new Number() { val = n.val * b.val };
        }
        [OperatorName(name = "assertEql")]
        public static NetLogoObject AssertEql(NetLogoObject n, NetLogoObject b)
        {
            string data = Environment.NewLine;
            data += "assertEql called in function: "+sys.GetCurrentFrame().FunctionName;
            data += Environment.NewLine;
            data += "Function lines of code:";
            data += Environment.NewLine; 
            foreach (string s in sys.GetCurrentFrame().baseFunction.body)
            {
                data += s;
                data += Environment.NewLine;
            }
            
            data += Environment.NewLine;
            data += "call stack frame:";
            data += Environment.NewLine;
            data += sys.GetCurrentFrame().ToString();
            data += Environment.NewLine;
            if (n.value.Equals(b.value))
            {
                data += ", Assert: True";
            }
            else
            {
                data += ", Assert: False";
            }
            data += Environment.NewLine;
            data += Environment.NewLine;
            File.AppendAllText("assert" + Process.GetCurrentProcess().StartTime.ToString("ddMMyyyyHHmm") + ".csv", data);
            return new NetLogoObject() { ptrID = "NULLPTR" };

        }
        [OperatorName(name = "setxy")]
        public static NetLogoObject Setxy(Number x, Number y)
        {
            if (sys.GetCurrentFrame().isAsk)
            {
                MetaAgent m = (MetaAgent)sys.Get("Agent");

                m.properties.SetProperty("X", new Integer() { value = x.value });
                m.properties.SetProperty("Y", new Integer() { value = y.value });
                m.x = (int)x.val;
                m.y = (int)y.val;


            }

            return new NetLogoObject() { ptrID = "NULLPTR" };
        }
        [OperatorName(name = "start-stopwatch")]
        public static NetLogoObject StopwatchStart(NetLogoObject o, NetLogoObject n)
        {
            Performance.PeformanceTracker.StartStopWatch((string)o.value);
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }

        [OperatorName(name = "stop-stopwatch",cast = false)]
        public static NetLogoObject StopwatchEnd(NetLogoObject o, NetLogoObject n)
        {
            o = sys.Assign((string)o.value, true);
           int val =  Performance.PeformanceTracker.StopWithoutWrite((string)o.value);
            Set(n, new Number() { value = val });
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }


        [OperatorName(cast = false,name = "random-xcor")]
        [OperatorName(cast = false, name = "random-ycor")]
        [OperatorName(cast = false, name = "random",treatAsVal = true)]
        public static Number Random(NetLogoObject o, NetLogoObject n)
        {
            int x = sys.r.Next(50);
            return new Number() { value = x };

        }
        [OperatorName(name = "clear-all")]
        public static NetLogoObject Reset(NetLogoObject o, NetLogoObject n)
        {
            sys.globals["ticks"].value = 0;
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }
        [OperatorName(name = "forward")]
        [OperatorName(name = "fd")]
        public static NetLogoObject Forward(Number o, NetLogoObject n)
        {

            if (sys.GetCurrentFrame().isAsk)
            {
                MetaAgent m = (MetaAgent)sys.Get("Agent");
                Integer x = new Integer();
                x.val = (int)(o.val * Math.Sin(Utils.MathUtilities.DegToRad(((Number)m.properties.GetProperty("rotation")).val)));
                Integer y = new Integer();
                y.val = (int)(o.val * Math.Cos(Utils.MathUtilities.DegToRad(((Number)m.properties.GetProperty("rotation")).val)));
                x.val = m.x + x.val;
                y.val = m.y + y.val;
                if ((x.val > 0) && (x.val < 49))
                {
                    m.properties.SetProperty("X", new Integer() { value = x.value });
                    m.x = (int)x.val;
                }
                if ((y.val > 0) && (y.val < 49))
                {

                    m.properties.SetProperty("Y", new Integer() { value = y.value });
                    m.y = (int)y.val;

                }
           




            }
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }
        [OperatorName(name = "rt")]
        public static NetLogoObject Rotate(Number o, NetLogoObject n)
        {

            if (sys.GetCurrentFrame().isAsk)
            {
                MetaAgent m = (MetaAgent)sys.Get("Agent");

                m.properties.SetProperty("rotation", new Number { val = o.val%360});

            }
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }
    }
}
