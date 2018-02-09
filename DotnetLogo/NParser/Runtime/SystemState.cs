using NParser.Types;
using NParser.Types.Agents;
using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Runtime
{
   public class SystemState
    {
        internal static SystemState internalState;
        public SystemState()
        {
            patches = new Patch[50,50];

            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    patches[i, j] = new Patch(i, j);

                }

            }

            internalState = this;
        }
        public Type checkType(string s)
        {
            if (char.IsDigit(s[0]))
            {
                return typeof(Number);
            }
            else if (char.IsLetter(s[0]))
            {
                return typeof(NetLogoObject);
            }
            else if (s[0] == '"')
            { return typeof(NSString); }

            return null;
        }

        public NetLogoObject Assign(string s,bool getValFromRef = true)
        {
            if (s != null)
            {
                if (char.IsDigit(s[0]))
                {
                    return new Number() { val = float.Parse(s) };
                }
                else if (char.IsLetter(s[0]))
                {
                    if (Get(s) != null && getValFromRef)
                    {
                        return Assign(GetVal(Get(s)));
                    }
                    else
                    {
                        return new NetLogoObject() { ptrID = s };
                    }
                 
                   
                }
                else if (s[0] == '"')
                { return new NSString() { val = s }; }
            }
            return new NetLogoObject() { ptrID = s };

        }

        public string GetVal(NetLogoObject o)
        {
            if (o is Number)
            {
                return ((Number)o).val.ToString();
            }
            else if (o is NSString)
            {
                return ((NSString)o).val;
            }
            return null;
        }

        public NetLogoObject Get(string s)
        {
            if (exeStack.Peek().param.ContainsKey(s))
            {
                return exeStack.Peek().param[s];
            }
            else if (exeStack.Peek().locals.ContainsKey(s))
            {
                return exeStack.Peek().locals[s];
            }
            else if (globals.ContainsKey(s))
            {
                return globals[s];
            }
            return null;
        }

        public Patch[,] patches;
        internal Dictionary<string,Function> registeredFunctions = new Dictionary<string, Function>();
        internal Stack<StackFrame> exeStack = new Stack<StackFrame>();
        internal Dictionary<string, NetLogoObject> globals = new Dictionary<string, NetLogoObject>();
        public void AddFunction(Function function)
        {
            registeredFunctions.Add(function.name, function);
        }

     
    }
}
