using NParser.Types;
using NParser.Types.Agents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NParser.Runtime
{
   public class SystemState
    {
        internal static SystemState internalState;
        public bool BreakExecution = false;
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
            bool b = false;
            if (s != null)
            {
                if (bool.TryParse(s, out b))
                {
                    return new Types.Boolean() { val = b };

                }
                if (char.IsDigit(s[0]))
                {
                    return new Number() { val = float.Parse(s) };
                }
                else if (s.StartsWith("color."))
                {
                    return new NSColour() { value = s };
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

        internal StackFrame GetCurrentFrame()
        {
            if (exeStack.Peek().anonymousFunction)
            {
                return GetLowerStackFunction();
            }
            else
            {
                return exeStack.Peek();
            }

        }


        public List<MetaAgent> GetBreed(string breed)
        {
            List<MetaAgent> data = new List<MetaAgent>();
            if (breed == "patches")
            {
                foreach (Patch p in patches)
                {
                    data.Add(p);
                }
            }
            else if (breed == "turtles")
            {
                data.AddRange(agents.Values);
            }
            return data;

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
            else
            {
                //DIRTY HACK TO GET REFRENCES !!
                return Assign(o.value.ToString()).value.ToString(); ;
            }
        }

        public void set(string s, NetLogoObject val)
        {
            if (GetCurrentFrame().isAsk)
            {
                MetaAgent m = (MetaAgent)GetCurrentFrame().param["Agent"];
                if (m.properties.GetProperty(s) != null)
                {
                m.properties.SetProperty(s,val);
                }
                else if (patches[m.x, m.y] != null)
                {
                    patches[m.x, m.y].properties.SetProperty(s,val);
                }


            }

        }

        internal StackFrame GetLowerStackFunction()
        {
          StackFrame s =  exeStack.Pop();
          StackFrame ret = GetCurrentFrame();
          exeStack.Push(s);
          return ret;
        }

        public NetLogoObject Get(string s)
        {
            if (GetCurrentFrame().param.ContainsKey(s))
            {
                return GetCurrentFrame().param[s];
            }
            else if (GetCurrentFrame().locals.ContainsKey(s))
            {
                return GetCurrentFrame().locals[s];
            }
            else if (globals.ContainsKey(s))
            {
                return globals[s];
            }
            if (GetCurrentFrame().isAsk)
            {
                MetaAgent m = (MetaAgent)GetCurrentFrame().param["Agent"];
                if (m.properties.GetProperty(s) != null)
                {
                    return (NetLogoObject)m.properties.GetProperty(s);
                }
                else if (patches[m.x, m.y] != null)
                {
                    return (NetLogoObject)patches[m.x, m.y].properties.GetProperty(s);
                }


            }
            return null;
        }
        public void PrintCallStack()
        {
            StackFrame f;
            List<StackFrame> tempQueue = new List<StackFrame>();
             var fh =  File.Create("StackTrace.txt");
            fh.Close();
            while (exeStack.Count > 0)
            {
                f = exeStack.Pop();
                tempQueue.Add(f);
                File.AppendAllText("StackTrace.txt", "***************" + exeStack.Count + "***************");
                File.AppendAllText("StackTrace.txt", Environment.NewLine);
                File.AppendAllText("StackTrace.txt", f.ToString());
                File.AppendAllText("StackTrace.txt", Environment.NewLine);
                File.AppendAllText("StackTrace.txt", "*************** END ***************");
                File.AppendAllText("StackTrace.txt", Environment.NewLine);
            }
            tempQueue.Reverse();
            foreach (StackFrame s in tempQueue )
            {
                exeStack.Push(s);
            }
                

            
           //exeStack = new Stack<StackFrame>( exeStack.Reverse());

        }

        public Patch[,] patches;
        internal Dictionary<string,Function> registeredFunctions = new Dictionary<string, Function>();
        internal Stack<StackFrame> exeStack = new Stack<StackFrame>();
        internal Dictionary<string, NetLogoObject> globals = new Dictionary<string, NetLogoObject>();
        public Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
        public void AddFunction(Function function)
        {
            registeredFunctions.Add(function.name, function);
        }

     
    }
}
