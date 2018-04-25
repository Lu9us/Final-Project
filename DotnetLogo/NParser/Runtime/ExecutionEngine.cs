using NParser.Runtime.DataStructs;
using NParser.Types;
using NParser.Types.Agents;
using NParser.Types.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NParser.Runtime.FlowControll;

namespace NParser.Runtime
{
    public class ExecutionEngine
    {
        public SystemState sys = new SystemState();
        public PreProcessor preProccessor;
        private bool skipToJump = false;

        public ExecutionEngine()
        {

            preProccessor = new PreProcessor(sys);
            SystemState.internalState = sys;
            StackFrame t = new StackFrame("INT", new Dictionary<string, NetLogoObject>(), null);
            OperatorFunctions.ResetSystemState();
            sys.exeStack.Push(t);
            sys.globals.Add("ticks", new Number() { val = 0 });
        }

        public void Load(string path)
        {
            preProccessor.LoadFile(path);
            while (!preProccessor.fileEnd)
                preProccessor.FirstPassRead();

        }
        public void Load(string[] data)
        {
            preProccessor.SetData(data);
            while (!preProccessor.fileEnd)
            {
                preProccessor.FirstPassRead();

            }

        }

        public void ExecuteTree(ParseTree t)
        {
#if LINETRACK || ALLTRACK
            Performance.PeformanceTracker.StartStopWatch(t.root.data);
#endif
            if (sys.GetCurrentFrame().FunctionName == "INT")
            {
                sys.GetCurrentFrame().pc = 0;
            }
            TreeNode n = ReadToLeaf(t.root);
            if (t.root.left == null && t.root.right == null && n == t.root)
            {
#if LINETRACK || ALLTRACK
                Performance.PeformanceTracker.Stop(t.root.data);
#endif

                t.root = new TreeNode(n.data + "ROOT");
                n.left = new TreeNode(n.data);
                n.left.parent = t.root;
                t.root.left = n.left;
                n = n.left;

#if LINETRACK || ALLTRACK
                Performance.PeformanceTracker.StartStopWatch(t.root.data);
#endif
            }
            Exec(n, t);
#if LINETRACK || ALLTRACK
            Performance.PeformanceTracker.Stop(t.root.data);
#endif



        }

        /// <summary>
        /// Back bone function to execute parse tree 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="t"></param>
        private void Exec(TreeNode n, ParseTree t)
        {
            try
            {
                if (sys.ExceptionThrown)
                {
                    return;
                }

                skipToJump = false;
                if (n == null)
                {
                    return;
                }

                if (n == t.root) //&& !t.IsOperator(n) && !sys.registeredFunctions.ContainsKey(n.data))
                {
                    return;
                }
                //Execute function
                else if (sys.registeredFunctions.ContainsKey(n.data))
                {
                    NodeExececution(n);
                    ExecFunction(n);
                }

                if (t.IsOperator(n))
                {
                    NodeExececution(n);
                    ExecOp(n);

                }
                //Build a agent create function from node
                if (n.data.StartsWith("create-"))
                {
                    //Get Function
                    Function f = sys.GetCurrentFrame().baseFunction;
                    StackFrame oldFrame = null;
                    int i = sys.GetCurrentFrame().pc + 1;
                    //Retrive agent create data from currently running function
                    AgentCreationStatement ac =
                        f.agentData.First(a => a.pcOffset == i && a.breed == n.data.Split('-')[1]);
                    int count = int.Parse((string)sys.Assign(ac.countVar).value.ToString());
                    //create agents
                    List<MetaAgent> l = new List<MetaAgent>();
                    for (int j = 0; j < count; j++)
                    {
                        l.Add(new Agent());
                    }
                    //for each agent construct a stack frame and execute it 
                    sys.GetCurrentFrame().pc = +ac.body.Count() + 1;
                    for (int k = 0; k < l.Count; k++)
                    {
                        //copy construct to not alter original data
                        Dictionary<string, NetLogoObject> param = new Dictionary<string, NetLogoObject>(sys.GetCurrentFrame().param);
                        Dictionary<string, NetLogoObject> vars = sys.GetCurrentFrame().locals;
                        param.Add("Agent", l[k]);
                        l[k].properties.SetProperty("rotation", new Number { val = sys.r.Next(360) });
                        StackFrame s = new StackFrame("NEWAGENT " + ac.breed, param
                            , ac)
                        { isAsk = true, locals = vars };
#if STACKTRACK || ALLTRACK
                 Performance.PeformanceTracker.StartStopWatch(s.FunctionName);
#endif
                        sys.exeStack.Push(s);
                        ParseTree pt;
                        while (s.pc < ac.body.Length && !sys.BreakExecution)
                        {
                            pt = new ParseTree(ac.body[s.pc]);



                            ExecuteTree(pt);
                            s.pc++;
                        }

                        oldFrame = sys.exeStack.Pop();
#if STACKTRACK || ALLTRACK
                        Performance.PeformanceTracker.Stop(oldFrame.FunctionName);
#endif
                    }
                    //add agents to the system state
                    foreach (MetaAgent a in l)
                    {
                        sys.agents.Add("Agent " + a.ID, (Agent)a);
                    }

#if StackTrace
  sys.PrintCallStack();
#endif

#if DEBUG
                    Console.WriteLine(oldFrame.ToString());
#endif
                    skipToJump = true;
                }
                //performe ask request on agents
                else if (n.data.StartsWith("ask"))
                {

                    string name;
                    Function f = sys.GetCurrentFrame().baseFunction;
                    name = n.left.data;

                    Ask a = f.askData.First(ab =>
                        ab.name == name && sys.GetCurrentFrame().pc == ab.pcOffset ||
                        sys.GetCurrentFrame().pc + 1 == ab.pcOffset);

                    List<MetaAgent> l = sys.GetBreed(name);

                    for (int i = 0; i < l.Count; i++)
                    {
                        //copy construct to not alter original data
                        Dictionary<string, NetLogoObject> param = new Dictionary<string, NetLogoObject>(sys.GetCurrentFrame().param);
                        Dictionary<string, NetLogoObject> vars = sys.GetCurrentFrame().locals;
                        if (param.ContainsKey("Agent") && sys.GetCurrentFrame().isAsk)
                        {
                            param["Agent"] = l[i];
                        }
                        else
                        {
                            param.Add("Agent", l[i]);
                        }

                        StackFrame ff = new StackFrame(name + "-ask",
                           param, a)
                        { isAsk = true, locals = vars };
                        ExecFrame(ff, a, n);
                    }

                    // sys.GetCurrentFrame().pc += a.body.Length + 1;
                    skipToJump = true;



                }
                // execute a branching statement
                else if (n.data.StartsWith("if") || n.data.StartsWith("elseif"))
                {
                    Function f = sys.GetCurrentFrame().baseFunction;
                    string line = n.parent.data.Replace('[', ' ');
                    FlowControll fc = f.flowControls.First(a => line.Contains(a.conditionalLine));
                    Dictionary<string, NetLogoObject> param = new Dictionary<string, NetLogoObject>(sys.GetCurrentFrame().param);
                    Dictionary<string, NetLogoObject> vars = sys.GetCurrentFrame().locals;
                    //observe result of the if block
                    if (bool.Parse(n.left.data))
                    {
                      //build a stack frame from the jump table of the current if block
                        StackFrame sf =
                            new StackFrame(f.name + "|" + fc.conditionalLine + "|" + FlowControll.JumpType.Succes,
                                param, fc.JumpTable[FlowControll.JumpType.Succes]);
                        sf.anonymousFunction = false;
                        sf.flowControl = true;
                        sf.isAsk = sys.GetCurrentFrame().isAsk;
                        sf.locals = vars;
                       
                        ExecFrame(sf, fc.JumpTable[FlowControll.JumpType.Succes], n);
                        sys.GetCurrentFrame().pc += fc.GetTotalJump();
                    }
                    else if (fc.type == "elseif")
                    {
                        //if the else block is an else if then it posesses a failure block and the if block has failed so use the fail block
                        StackFrame sf =
                             new StackFrame(f.name + "|" + fc.conditionalLine + "|" + FlowControll.JumpType.Fail,
                               param, fc.JumpTable[FlowControll.JumpType.Fail]);
                        sf.anonymousFunction = false;
                        sf.flowControl = true;
                        sf.isAsk = sys.GetCurrentFrame().isAsk;
                        sf.locals = vars;

                        ExecFrame(sf, fc.JumpTable[FlowControll.JumpType.Fail], n);
                        sys.GetCurrentFrame().pc += fc.GetTotalJump();
                    }
                    else
                    {
                        sys.GetCurrentFrame().pc += fc.GetTotalJump();
                    }
                       
                    
                    //  ParseTree pt = new ParseTree(fc.conditionalLine);
                    //   ExecuteTree(pt);

                    skipToJump = true;
                }

                if (!skipToJump)
                {
                    Exec(n.parent, t);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                sys.PrintCallStack();
                sys.ExceptionThrown = true;
                return;
            }
        }

        public void ExecForPatch(int x, int y, TreeNode n)
        {
            if (sys.patches.GetLength(0) < x && sys.patches.GetLength(1) < y)
            {
                MetaAgent m = sys.patches[x, y];
                
            }
            
        }
        /// <summary>
        ///  Get value from varible identity
        /// </summary>
        /// <param name="n"></param>
        public void GetVar(TreeNode n)
        {
            TreeNode temp = new TreeNode(sys.GetVal(sys.Get(n.data)));
            if (n.parent.left == n)
            {
                n.parent.left = temp;
            }
            else if (n.parent.right == n)
            {
                n.parent.right = temp;
            }
            temp.parent = n.parent;
        }
        /// <summary>
        /// Decide on how to execute tree node 
        /// </summary>
        /// <param name="n">Node to execute</param>
        public void NodeExececution(TreeNode n)
        {

      
             if (n.left != null && (sys.registeredFunctions.ContainsKey(n.left.data) || isOp(n.left)))
            {
                if (isOp(n.left))
                {
                    ExecOp(n.left);
                }
                else
                {
                    ExecFunction(n.left);
                }
            }
          
            if (n.right != null && (sys.registeredFunctions.ContainsKey(n.right.data) || isOp(n.right)))
            {
                if (isOp(n.right))
                {
                    ExecOp(n.right);
                }
                else
                {
                    ExecFunction(n.right);
                }
            }
        }


        /// <summary>
        /// Execute a stack frame
        /// </summary>
        /// <param name="fFrame">Stack frame to execute</param>
        /// <param name="f">The function of which the stack frame is based</param>
        /// <param name="n">TreeNode being executed</param>
        public void ExecFrame(StackFrame fFrame, Function f, TreeNode n)
        {
#if STACKTRACK || ALLTRACK
            Performance.PeformanceTracker.StartStopWatch(fFrame.FunctionName);
#endif

            fFrame.pc = 0;
            sys.exeStack.Push(fFrame);
            ParseTree pt;
#if StackTrace
            sys.PrintCallStack();
#endif
            //execute stack frame till end
            while (fFrame.pc < f.body.Length && !sys.BreakExecution)
            {
                pt = new ParseTree(f.body[fFrame.pc]);



                ExecuteTree(pt);
                if (sys.ExceptionThrown)
                {

                    return;

                }

                fFrame.pc++;
            }
#if DEBUG
            if (sys.BreakExecution)
            {
                Console.WriteLine("Execution broken : " + sys.BreakExecution);
            }

#endif
            //if there is a value to be reported place it into the parse tree or remove node from parse tree 
            if (f.Report && sys.GetCurrentFrame().ReportValue != null)
            {
                sys.BreakExecution = false;
                TreeNode tempNode = new TreeNode(sys.Assign(sys.GetCurrentFrame().ReportValue.value.ToString()).value.ToString());
                if (n.parent.left == n)
                {
                    n.parent.left = tempNode;
                }
                else if (n.parent.right == n)
                {
                    n.parent.right = tempNode;
                }
                tempNode.parent = n.parent;

            }
            else
            {
                if (n.parent != null)
                {
                    if (n.parent.left == n)
                    {
                        n.parent.left = null;
                    }
                    else if (n.parent.right == n)
                    {
                        n.parent.right = null;
                    }
                }
                else
                {
                    n = null;
                }
            }

            //pop the stack and the jump last function to the end of the current function
            StackFrame oldFrame = sys.exeStack.Pop();
            if (f is Ask  || f is AgentCreationStatement)
            {
                sys.GetCurrentFrame().pc = oldFrame.pc + f.pcOffset;
            }

#if StackTrace
            Console.WriteLine(oldFrame.ToString());
#endif
#if STACKTRACK || ALLTRACK
            Performance.PeformanceTracker.Stop(fFrame.FunctionName);
#endif
        }

        /// <summary>
        /// Execute a function based on tree node passed in
        /// </summary>
        /// <param name="n">tree node for function </param>
        public void ExecFunction(TreeNode n)
        {
            Function f = sys.registeredFunctions[n.data];
            bool ask = false;


            sys.BreakExecution = false;
            List<NetLogoObject> vals = GetParams(n, f.name);
           
            Dictionary<string, NetLogoObject> objects = new Dictionary<string, NetLogoObject>();
            //if ask pass agent into the paraters
            if (sys.GetCurrentFrame().isAsk)
            {

                ask = true;
                objects.Add("Agent",sys.GetCurrentFrame().param["Agent"]);
            }
            //pair paramaters with names
            for (int i = 0; i < f.paramaters.Count; i++)
            {
                objects.Add(f.paramaters[i], vals[i]);
            }
            //build and execute stack frame
            StackFrame fFrame = new StackFrame(f.name,objects, f) { Report = f.Report,isAsk = ask };

            fFrame.pc = 0;
            ExecFrame(fFrame, f, n);
        }
        /// <summary>
        /// Get paramaters for function
        /// </summary>
        /// <param name="n"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private List<NetLogoObject> GetParams(TreeNode n, string functionName)
        {
            List<NetLogoObject> paramList = new List<NetLogoObject>();




            if (n.data != functionName)
            {
                paramList.Add(sys.Assign(n.data));
            }

            if (n.left != null)
            {
                paramList.AddRange(GetParams(n.left, functionName));
            }
            else if (n.left == null && n.right != null && functionName == n.data)
            {
                paramList.Add(sys.Assign(n.parent.right.data));
            }
            if (n.right != null)
            {
                paramList.AddRange(GetParams(n.right, functionName));
            }

            return paramList;
        }




        /// <summary>
        /// Does tree node have both parents 
        /// </summary>
        /// <param name="n">Tree node</param>
        /// <returns></returns>
        public bool nodeFull(TreeNode n)
        {
            if (n.left != null && n.right != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Execute operator
        /// </summary>
        /// <param name="n">tree node</param>
        public void ExecOp(TreeNode n)
        {

            NetLogoObject obj = null;
            //create parse tree to get operator list THIS IS A HACK
            ParseTree t = new ParseTree("");
            try
            {
                if (nodeFull(n))
                {
                    if (t.IsOperator(n.left))
                    {
                        ExecOp(n.left);

                    }

                    if (t.IsOperator(n.right))
                    {
                        ExecOp(n.right);

                    }

#if OPTRACK || ALLTRACK
                    Performance.PeformanceTracker.StartStopWatch(n.data);
#endif

                    obj = OperatorTable.Call<NetLogoObject>(sys.Assign(n.left.data, false),
                        sys.Assign(n.right.data, false), n.data);


                }
                else if (n.left != null)
                {

                    obj = OperatorTable.Call<NetLogoObject>(sys.Assign(n.left.data, false),
                        new NetLogoObject() { ptrID = "NULLPTR" }, n.data);

                }
                else
                {

                    obj = OperatorTable.Call<NetLogoObject>(new NetLogoObject() { ptrID = "NULLPTR" },
                        new NetLogoObject() { ptrID = "NULLPTR" }, n.data);

                }
                //place results of the operation on tree
                TreeNode tempNode = new TreeNode(obj.value.ToString());
#if OPTRACK || ALLTRACK
                Performance.PeformanceTracker.Stop(n.data);
#endif
                //replace node
                if (n.parent != null)
                {
                    if (n.parent.left == n)
                    {
                        n.parent.left = tempNode;
                    }
                    else if (n.parent.right == n)
                    {
                        n.parent.right = tempNode;
                    }

                    tempNode.parent = n.parent;
                }
            }
            catch (Exception e)
            {
                throw e;

            }
        }
        /// <summary>
        /// Checks if node is operator
        /// </summary>
        /// <param name="n">node to check</param>
        /// <returns></returns>
        private bool isOp(TreeNode n)
        {
            //hack
            ParseTree tr = new ParseTree("");
            return tr.IsOperator(n);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private TreeNode ReadToLeaf(TreeNode n)
        {
            if (n.left == null && n.right == null)
            {
                return n;
            }
            if (n.right != null)
            {
                return ReadToLeaf(n.right);
            }
            else if (n.left != null)
            {

                return ReadToLeaf(n.left);
            }
            return null;

        }


    }


}

