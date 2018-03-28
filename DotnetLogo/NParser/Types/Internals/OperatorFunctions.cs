using NParser.Runtime;
using NParser.Types.Agents;
using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types.Internals
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OperatorName : Attribute
    {
        public string name { get; set; }
        public bool cast { get; set; } = true;
    }

    public static class OperatorFunctions
    {
        private static Random r = new Random(DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year);
        private static SystemState sys = SystemState.internalState;

        public static void ResetSystemState()
        {
            sys = SystemState.internalState;
        }
        [OperatorName(cast = false, name = "if")]
        public static Boolean IF(Boolean o, NetLogoObject n)
        {

            return o;

        }
        [OperatorName(cast  = false,name ="=")]
        public static Boolean equal(NetLogoObject o, NetLogoObject n)
        {
            if (o.value == o.value)
            {
                return new Boolean() { val = true };
            }
            else
            {
                return new Boolean() { val = false };
            }
        }
        [OperatorName]
        public static Number mod(Number n, Number o)
        {

            return new Number() { val = n.val % o.val };

        }

        [OperatorName(cast = false, name = "tick")]
        public static NetLogoObject tick(NetLogoObject o, NetLogoObject n)
        {

            sys.globals["ticks"].value = ((float)sys.globals["ticks"].value) + 1;
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }

        [OperatorName(cast = false, name = "let")]
        public static NetLogoObject let(NetLogoObject o, NetLogoObject n)
        {
            if (sys.GetCurrentFrame().isAsk)
            {
                var v = (MetaAgent)sys.GetCurrentFrame().param["Agent"];

                v.properties.AddProperty((string)o.value, n);

                return new NetLogoObject() { ptrID = "NULLPTR" };
            }
            else
            {
                //  var v = sys.Assign(n.value.ToString());
                sys.GetCurrentFrame().locals.Add((string)o.value, n);
            }
            return new NetLogoObject() { ptrID = o.ptrID };
        }

        [OperatorName(cast = false, name = "report")]
        public static NetLogoObject report(NetLogoObject o, NetLogoObject d)
        {
            sys.GetCurrentFrame().ReportValue = o;
            sys.BreakExecution = true;
            return new NetLogoObject() { ptrID = o.value.ToString() };

        }

        [OperatorName(name = "show")]
        public static NetLogoObject show(NetLogoObject o, NetLogoObject d)
        {
            Console.WriteLine(o.value);
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }

        [OperatorName(cast = false, name = "set")]
        public static NetLogoObject set(NetLogoObject o, NetLogoObject n)
        {
            if (sys.GetCurrentFrame().isAsk)
            {
                var v = (MetaAgent)sys.GetCurrentFrame().param["Agent"];

                if (o.ptrID != null)
                {
                    sys.set((string)o.value, n);
                }
                else
                {
                    sys.set((string)n.value, o);
                }


                return new NetLogoObject() { ptrID = "NULLPTR" };
            }
            else
            {
                sys.GetCurrentFrame().locals[(string)o.value] = n;
            }

            return new NetLogoObject() { ptrID = o.ptrID };

        }
        [OperatorName(name = "+")]
        public static Number add(Number n, Number b)
        {
            return new Number() { val = n.val + b.val };
        }
        [OperatorName(name = "/")]
        public static Number divide(Number n, Number b)
        {
            return new Number() { val = n.val / b.val };
        }
        [OperatorName(name = "-")]
        public static Number sub(Number n, Number b)
        {
            return new Number() { val = n.val - b.val };
        }
        [OperatorName(name = "*")]
        public static Number order(Number n, Number b)
        {
            return new Number() { val = n.val * b.val };
        }
        [OperatorName(name = "setxy")]
        public static NetLogoObject setxy(Number x, Number y)
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
        [OperatorName(cast = false,name = "random-xcor")]
        [OperatorName(cast = false, name = "random-ycor")]
        [OperatorName(cast = false, name = "random")]
        public static Number random(NetLogoObject o, NetLogoObject n)
        {
            int x = r.Next(20);
            return new Number() { value = x };

        }
        [OperatorName(name = "clear-all")]
        public static NetLogoObject reset(NetLogoObject o, NetLogoObject n)
        {
            sys.globals["ticks"].value = 0;
            return new NetLogoObject() { ptrID = "NULLPTR" };
        }
        [OperatorName(name = "forward")]
        [OperatorName(name = "fd")]
        public static NetLogoObject forward(Number o, NetLogoObject n)
        {

            if (sys.GetCurrentFrame().isAsk)
            {
                MetaAgent m = (MetaAgent)sys.Get("Agent");
                Integer x = new Integer();
                x.val = (int)(o.val * Math.Sin(((Number)m.properties.GetProperty("rotation")).val));
                Integer y = new Integer();
                y.val = (int)(o.val * Math.Cos(((Number)m.properties.GetProperty("rotation")).val));
                x.val = m.y + x.val;
                y.val = m.y + x.val;
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
        public static NetLogoObject rotate(Number o, NetLogoObject n)
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
