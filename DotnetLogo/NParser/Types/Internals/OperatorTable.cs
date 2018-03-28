using NParser.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
namespace NParser.Types.Internals
{
    /// <summary>
    /// struct 
    /// </summary>
    public struct OpPair
    {
        public OpPair(Type a, Type b,Type rturn,string s,bool cast =  true)
        {
            opL = a;
            opR = b;
            ret = rturn;
            token = s;
            this.cast = cast;
        }
        public string token;
        public bool cast;
        public Type opL;
        public Type opR;
        public Type ret;


    }

    internal static class OperatorTable
    {
        public delegate t3 opFunct<t1, t2,t3>(t1 a, t2 b);

        public static Dictionary<OpPair, Delegate> opTable = new Dictionary<OpPair, Delegate>();

        internal static t3 Call<t3>(NetLogoObject a, NetLogoObject b,string s)
        {


            if (a.GetType() == typeof(NetLogoObject) && !string.IsNullOrEmpty(a.ptrID) && a.ptrID != "NULLPTR" && opTable.Any(t => t.Key.token == s && t.Key.cast)) //&& v.cast)
            {
                a = SystemState.internalState.Assign(a.value.ToString());
            }
            else
            {
                a = SystemState.internalState.Assign(a.value.ToString(), false);
            }

            if (b.GetType() == typeof(NetLogoObject) && !string.IsNullOrEmpty(b.ptrID) && b.ptrID != "NULLPTR" && opTable.Any(t => t.Key.token == s && t.Key.cast))//&& v.cast)
            {
                b = SystemState.internalState.Assign(b.value.ToString());
            }
            else
            {
                b = SystemState.internalState.Assign(b.value.ToString(), false);
            }

            if (b == null)
            {
                b = new NetLogoObject() { ptrID = "NULLPTR" };
            }


           
#if DEBUGOPS
            foreach (OpPair c in opTable.Keys)
            {
                Console.Write(b.GetType()); Console.Write("   "); Console.Write(b.GetType().IsSubclassOf(c.opR) || b.GetType() == c.opR); Console.Write("   ");
                Console.Write(a.GetType());Console.Write("   "); Console.Write(a.GetType().IsSubclassOf(c.opL) || a.GetType() == c.opL); Console.Write("   "); Console.Write(c.token); Console.Write("   ");  Console.Write(c.token == s) ;
                Console.WriteLine();
            }

#endif

            Delegate d = opTable.First(c => (a.GetType().IsSubclassOf(c.Key.opL) || a.GetType() == c.Key.opL) && (b.GetType().IsSubclassOf(c.Key.opR) || b.GetType() == c.Key.opR) && c.Key.token == s  ).Value;
            OpPair v =  opTable.First(c => (a.GetType().IsSubclassOf(c.Key.opL) || a.GetType() == c.Key.opL) && (b.GetType().IsSubclassOf(c.Key.opR) || b.GetType() == c.Key.opR) && c.Key.token == s).Key;
            if (d != null)
            {

                try
                {                 
                    return (t3)d.DynamicInvoke(new[] { a, b });
                }
                catch (Exception e)
                {
                    throw new RTException("Error occured during operator evaluation ERROR: " + e.Message);
                }
            }
            return default(t3);
        }

        static OperatorTable()
        {

           Type t = typeof(OperatorFunctions);
            Type methodPtr = typeof(opFunct<,,>);

           var v = t.GetMethods();
            foreach (var method in v)
            {
                if (method.IsStatic)
                {
                 var paramArray=   method.GetParameters();
                 var returnid = method.ReturnParameter;
                 var operatorAtt =   method.GetCustomAttributes().Where(a=>a is OperatorName);
                    foreach (var op in operatorAtt)
                    {
                        Type constructedmethodPtr = methodPtr.MakeGenericType(new[] { paramArray[0].ParameterType, paramArray[1].ParameterType, returnid.ParameterType });
                        OpPair o = new OpPair(paramArray[0].ParameterType, paramArray[1].ParameterType, returnid.ParameterType, ((OperatorName)op).name, ((OperatorName)op).cast);
                        Delegate created = Delegate.CreateDelegate(constructedmethodPtr,method);
                        opTable.Add(o, created);
                    }
                   
                }

            }
           

/*
            opFunct<NetLogoObject, NetLogoObject, NetLogoObject> temp = OperatorFunctions.let;
            opTable.Add(new OpPair(typeof(NetLogoObject), typeof(NetLogoObject), typeof(NetLogoObject), "let",false),temp);
            temp = OperatorFunctions.report;
            opTable.Add(new OpPair(typeof(NetLogoObject), typeof(NetLogoObject), typeof(NetLogoObject), "report", false), temp);
            temp = OperatorFunctions.tick;
            opTable.Add(new OpPair(typeof(NetLogoObject), typeof(NetLogoObject), typeof(NetLogoObject), "tick", false), temp);
            temp = OperatorFunctions.set;
            opTable.Add(new OpPair(typeof(NetLogoObject), typeof(NetLogoObject), typeof(NetLogoObject), "set", false), temp);
            temp = OperatorFunctions.show;
            opTable.Add(new OpPair(typeof(NetLogoObject), typeof(NetLogoObject), typeof(NetLogoObject), "show", true), temp);
            opFunct<Number, Number, NetLogoObject> tempAdd = OperatorFunctions.add;
            opTable.Add(new OpPair(typeof(Number), typeof(Number), typeof(NetLogoObject), "+"), tempAdd);
            tempAdd = OperatorFunctions.sub;
            opTable.Add(new OpPair(typeof(Number), typeof(Number), typeof(NetLogoObject), "-"), tempAdd);
            tempAdd = OperatorFunctions.divide;
            opTable.Add(new OpPair(typeof(Number), typeof(Number), typeof(NetLogoObject), "/"), tempAdd);
            tempAdd = OperatorFunctions.order;
            opTable.Add(new OpPair(typeof(Number), typeof(Number), typeof(NetLogoObject), "*"), tempAdd);
            tempAdd = OperatorFunctions.setxy;
            opTable.Add(new OpPair(typeof(Number), typeof(Number), typeof(NetLogoObject), "setxy"), tempAdd);
            temp = OperatorFunctions.reset;
            opTable.Add(new OpPair(typeof(NetLogoObject), typeof(NetLogoObject), typeof(NetLogoObject), "clear-all", false), temp);
            temp = OperatorFunctions.reset;
            opFunct<NetLogoObject, NetLogoObject, Number> tempN = OperatorFunctions.random;
            opTable.Add(new OpPair(typeof(NetLogoObject), typeof(NetLogoObject), typeof(Number), "random-xcor", false), tempN);
            tempN = OperatorFunctions.random;
            opTable.Add(new OpPair(typeof(NetLogoObject), typeof(NetLogoObject), typeof(Number), "random-ycor", false), tempN);
            */
        }

    }

    
    

}
