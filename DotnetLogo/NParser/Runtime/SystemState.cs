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
        public bool ExceptionThrown = false;
        public Patch[,] patches;
        internal Random r = new Random(Guid.NewGuid().GetHashCode());
        internal Dictionary<string, Function> registeredFunctions = new Dictionary<string, Function>();
        internal Stack<StackFrame> exeStack = new Stack<StackFrame>();
        internal Dictionary<string, NetLogoObject> globals = new Dictionary<string, NetLogoObject>();
        public Dictionary<string, Agent> agents = new Dictionary<string, Agent>();

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
        /// <summary>
        /// Get the type of a piece of data dependent on its format
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
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
        /// <summary>
        /// ceate a logo object for varible type passed in  
        /// </summary>
        /// <param name="s"></param>
        /// <param name="getValFromRef"></param>
        /// <returns></returns>
        public NetLogoObject Assign(string s,bool getValFromRef = true)
        {
            bool b = false;
            try
            {
                if (s != null)
                {
                    if (bool.TryParse(s, out b))
                    {
                        return new Types.Boolean() {val = b};

                    }

                    if (char.IsDigit(s[0]))
                    {
                        return new Number() {val = float.Parse(s)};
                    }
                    else if (s.StartsWith("color."))
                    {
                        return new NSColour() {value = s};
                    }
                    else if (char.IsLetter(s[0]))
                    {
                        if (Get(s) != null && getValFromRef)
                        {
                            return Assign(GetVal(Get(s)));
                        }
                        else
                        {
                            return new NetLogoObject() {ptrID = s};
                        }


                    }
                    else if (s[0] == '"')
                    {
                        return new NSString() {val = s};
                    }

                }

                return new NetLogoObject() {ptrID = s};
            }
            catch (Exception e)
            {
                ExceptionThrown = true;
                throw new RTException("Type missmatch occured: "+e.Message+Environment.NewLine+exeStack.Peek().ToString());
              
            }

        }
        /// <summary>
        /// Retrives a list of patches within 1 unit around a X Y point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal  List<MetaAgent> GetNeighbours(int x, int y)
        {
            List<MetaAgent> list = new List<MetaAgent>();

            for (int i = (int)x - 1; i < x + 2; i++)
            {
                for (int j = (int)y - 1; j < y + 2; j++)
                {
                    if (i < patches.GetLongLength(0) && i > -1 && j < patches.GetLongLength(1) && j > -1)
                    {
                        if (!(i == x && j == y))
                        {
                            list.Add(patches[i, j]);
                        }
                    }
                }

            }
            return list;
        }

        /// <summary>
        /// returns the current stack frame
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Returns data for ask requests 
        /// </summary>
        /// <param name="breed"></param>
        /// <returns></returns>
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
            else if (breed == "neighbours")
            {
                if (GetCurrentFrame().isAsk)
                {
                    MetaAgent a = (MetaAgent)GetCurrentFrame().param["Agent"];
                  return  GetNeighbours(((Integer)a.properties.GetProperty("X")).val, ((Integer)a.properties.GetProperty("Y")).val);
                    
                }
                else
                {
                    throw new RTException("neighbours request passed with no agent");
                }
            }
            return data;

        }
        /// <summary>
        /// Gets a value from a Logo object 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Unwinds the stack to the first non anonymous functions
        /// </summary>
        /// <returns></returns>
        public StackFrame UnwindAnonFunctions()
        {
            List<StackFrame> tempQueue = new List<StackFrame>();
            StackFrame sff;
            while (exeStack.Count > 0)
            {
                sff = exeStack.Pop();
                tempQueue.Add(sff);
                if (registeredFunctions.ContainsKey(sff.FunctionName))
                {
                    tempQueue.Reverse();
                    foreach (StackFrame s in tempQueue)
                    {
                        exeStack.Push(s);
                    }
                    return sff;
                }
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="val"></param>
        public void set(string s, NetLogoObject val)
        {
            if (GetCurrentFrame().isAsk)
            {
                MetaAgent m = (MetaAgent)GetCurrentFrame().param["Agent"];

                if (GetCurrentFrame().locals.ContainsKey(s))
                {
                    GetCurrentFrame().locals[s] = val;
                }

               else if (patches[m.x, m.y] != null && patches[m.x, m.y].properties.properties.ContainsKey(s))
                {
                    if (patches[m.x, m.y].properties.properties.ContainsKey(s))
                    {
                        patches[m.x, m.y].properties.SetProperty(s, val);
                    }
                }

                else if(m.properties.properties.ContainsKey(s))
                {
                    m.properties.SetProperty(s, val);
                }
                

            }
            else
            {
                GetCurrentFrame().locals[s] = val;
            }
        }
        /// <summary>
        /// get the stack frame below the current
        /// </summary>
        /// <returns></returns>
        internal StackFrame GetLowerStackFunction()
        {
          StackFrame s =  exeStack.Pop();
          StackFrame ret = GetCurrentFrame();
          exeStack.Push(s);
          return ret;
        }
        /// <summary>
        /// Get a varible from an identity 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Prints out the current call stack
        /// </summary>
        public void PrintCallStack()
        {
            StackFrame f;
            List<StackFrame> tempQueue = new List<StackFrame>();
#if StackToFile
            var fh =  File.Create("StackTrace.txt");
            fh.Close();
#endif
            Console.WriteLine("Dumping CallStack");
            while (exeStack.Count > 0)
            {
                f = exeStack.Pop();

                tempQueue.Add(f);
#if StackToFile
                File.AppendAllText("StackTrace.txt", "***************" + exeStack.Count + "***************");
                File.AppendAllText("StackTrace.txt", Environment.NewLine);
                File.AppendAllText("StackTrace.txt", f.ToString());
                File.AppendAllText("StackTrace.txt", Environment.NewLine);
                File.AppendAllText("StackTrace.txt", "*************** END ***************");
                File.AppendAllText("StackTrace.txt", Environment.NewLine);
#else
               
               Console.WriteLine( "***************" + exeStack.Count + "***************");
               Console.WriteLine( Environment.NewLine);
               Console.WriteLine( f.ToString());
               Console.WriteLine( Environment.NewLine);
               Console.WriteLine( "*************** END ***************");
               Console.WriteLine( Environment.NewLine);
#endif
            }
            tempQueue.Reverse();
            foreach (StackFrame s in tempQueue )
            {
                exeStack.Push(s);
            }
                

            
           //exeStack = new Stack<StackFrame>( exeStack.Reverse());

        }

        
        public void AddFunction(Function function)
        {
            registeredFunctions.Add(function.name, function);
        }

     
    }
}
