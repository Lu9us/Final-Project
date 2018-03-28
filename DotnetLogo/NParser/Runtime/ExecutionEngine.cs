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
            StackFrame t = new StackFrame("INT", new Dictionary<string, NetLogoObject>());
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
                preProccessor.FirstPassRead();


        }

        public void ExecuteTree(ParseTree t)
        {
            if (sys.GetCurrentFrame().FunctionName == "INT")
            {
                sys.GetCurrentFrame().pc = 0;
            }
            TreeNode n = ReadToLeaf(t.root);
            if (t.root.left == null && t.root.right == null&& n == t.root)
            {
                t.root = new TreeNode(n.data + "ROOT");
                n.left = new TreeNode(n.data);
                n.left.parent = t.root;
                t.root.left = n.left;
                n = n.left;
            }
            Exec(n, t);



        }
       

        private void Exec(TreeNode n, ParseTree t)
        {
            

            skipToJump = false;
            if (n == null)
            {
                return;
            }

            if (n == t.root )//&& !t.IsOperator(n) && !sys.registeredFunctions.ContainsKey(n.data))
            {
                return;
            }
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

            if (n.data.StartsWith("create-"))
            {
                Function f = sys.registeredFunctions[sys.GetCurrentFrame().FunctionName];
                StackFrame oldFrame = null;
                int i = sys.GetCurrentFrame().pc + 1;
                AgentCreationStatement ac = f.agentData.First(a => a.startOffset == i && a.breed == n.data.Split('-')[1]);
                int count = int.Parse((string)sys.Assign(ac.countVar).value.ToString());
                List<MetaAgent> l = new List<MetaAgent>();
                for (int j = 0; j < count; j++)
                {
                    l.Add(new Agent());
                }
                sys.GetCurrentFrame().pc = +ac.lines.Count() + 1;
                for (int k = 0; k < l.Count; k++)
                {
                    StackFrame s = new StackFrame("NEWAGENT " + ac.breed, new Dictionary<string, NetLogoObject>() { { "Agent", l[k] } }) { isAsk = true };

                    sys.exeStack.Push(s);
                    ParseTree pt;
                    while (s.pc < ac.lines.Length && !sys.BreakExecution)
                    {
                        pt = new ParseTree(ac.lines[s.pc]);



                        ExecuteTree(pt);
                        s.pc++;
                    }
                    oldFrame = sys.exeStack.Pop();
                }
                foreach (MetaAgent a in l)
                {
                    sys.agents.Add("Agent " + a.ID, (Agent)a);
                }

#if DEBUG
                //  sys.PrintCallStack();
#endif

                //sys.GetCurrentFrame().pc += oldFrame.pc;
#if DEBUG
                Console.WriteLine(oldFrame.ToString());
#endif
                skipToJump = true;
            }
            else if (n.data.StartsWith("ask"))
            {
                Function f = null;
                string name;
                if (sys.GetCurrentFrame().flowControl)
                {
                    string[] flowControll = sys.GetCurrentFrame().FunctionName.Split('|');
                    f = sys.registeredFunctions[flowControll[0]].flowControls.First(fc=>fc.conditionalLine.Contains(flowControll[1])).JumpTable[(JumpType)Enum.Parse(typeof(JumpType), flowControll[2])];
                }
                else
                {
                   f = sys.registeredFunctions[sys.GetCurrentFrame().FunctionName];
                }
                name = n.left.data;
               
                Ask a = f.askData.First(ab => ab.name == name && sys.GetCurrentFrame().pc == ab.pcOffset || sys.GetCurrentFrame().pc + 1 == ab.pcOffset);

                List<MetaAgent> param = sys.GetBreed(name);

                for (int i = 0; i < param.Count; i++)
                {
                    StackFrame ff = new StackFrame(name + "-ask", new Dictionary<string, NetLogoObject> { { "Agent", param[i] } }) { isAsk = true };
                    ExecFrame(ff, a, n);
                }
                sys.GetCurrentFrame().pc += a.pcOffset;
                skipToJump = true;



            }
            else if (n.data.StartsWith("if")||n.data.StartsWith("elseif"))
            {
                Function f = sys.registeredFunctions[sys.GetCurrentFrame().FunctionName];
                string line = n.parent.data.Replace('[', ' ');
                FlowControll fc = f.flowControls.First(a =>line.Contains(a.conditionalLine));
               
                if (bool.Parse(n.left.data))
                {
                    StackFrame sf = new StackFrame(f.name + "|" + fc.conditionalLine+"|"+FlowControll.JumpType.Succes, sys.GetCurrentFrame().param);
                    sf.anonymousFunction = false;
                    sf.flowControl = true;
                    sf.isAsk = sys.GetCurrentFrame().isAsk;
                    ExecFrame(sf, fc.JumpTable[FlowControll.JumpType.Succes], n);
                }
                else if (fc.type == "elseif")
                {
                    StackFrame sf = new StackFrame(f.name + "|" + fc.conditionalLine + "|" + FlowControll.JumpType.Fail, sys.GetCurrentFrame().param);
                    sf.anonymousFunction = false;
                    sf.flowControl = true;
                    sf.isAsk = sys.GetCurrentFrame().isAsk;
                    ExecFrame(sf, fc.JumpTable[FlowControll.JumpType.Fail], n);
                }
                //  ParseTree pt = new ParseTree(fc.conditionalLine);
                //   ExecuteTree(pt);
                sys.GetCurrentFrame().pc += fc.GetTotalJump();
                skipToJump = true;
            }
            if (!skipToJump)
            {
                Exec(n.parent, t);
            }
        }

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

        public void NodeExececution(TreeNode n)
        {

            if (n.left != null && (sys.registeredFunctions.ContainsKey(n.left.data) || isOp(n.left)) && n.right != null)
            {
                //TreeNode ns = ReadToLeaf(n.left);
                //ns.left = new TreeNode(n.right.data);
                //ns.left.parent = ns;
                //n.right = null;
                if (isOp(n.left))
                {
                    ExecOp(n.left);
                }
                else
                {
                    ExecFunction(n.left);
                }

            }
            else if (n.left != null && (sys.registeredFunctions.ContainsKey(n.left.data) || isOp(n.left)))
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
           /* if (n.right != null && (sys.registeredFunctions.ContainsKey(n.right.data) || isOp(n.right)) && n.left != null)
            {
                TreeNode ns = ReadToLeaf(n.right);
                ns.right = new TreeNode(n.right.data);
                ns.right.parent = ns;
                n.left = null;
                if (isOp(n.right))
                {
                    ExecOp(n.right);
                }
                else
                {
                    ExecFunction(n.right);
                }

            }
            else*/ if (n.right != null && (sys.registeredFunctions.ContainsKey(n.right.data) || isOp(n.right)))
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



        public void ExecFrame(StackFrame fFrame, Function f, TreeNode n)
        {

            fFrame.pc = 0;
            sys.exeStack.Push(fFrame);
            ParseTree pt;
            sys.PrintCallStack();
            while (fFrame.pc < f.body.Length && !sys.BreakExecution)
            {
                pt = new ParseTree(f.body[fFrame.pc]);



                ExecuteTree(pt);
                fFrame.pc++;
            }
#if DEBUG 
            Console.WriteLine("Execution broken : " + sys.BreakExecution);
#endif 

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


            StackFrame oldFrame = sys.exeStack.Pop();
            sys.GetCurrentFrame().pc = oldFrame.pc+1+f.pcOffset;
#if DEBUG
            Console.WriteLine(oldFrame.ToString());
#endif

        }


        public void ExecFunction(TreeNode n)
        {
            Function f = sys.registeredFunctions[n.data];

         
       
            sys.BreakExecution = false;
            List<NetLogoObject> vals = GetParams(n, f.name);
            
            Dictionary<string, NetLogoObject> objects = new Dictionary<string, NetLogoObject>();
            for (int i = 0; i < f.paramaters.Count; i++)
            {
                objects.Add(f.paramaters[i], vals[i]);
            }
            StackFrame fFrame = new StackFrame(f.name, objects) { Report = f.Report };

            fFrame.pc = 0;
            ExecFrame(fFrame, f, n);
        }
        private List<NetLogoObject> GetParams(TreeNode n,string functionName)
        {
            List<NetLogoObject> paramList = new List<NetLogoObject>();
            
            


            if (n.data != functionName)
            {
                paramList.Add(sys.Assign(n.data));
            }

            if (n.left != null )
            {
                paramList.AddRange(GetParams(n.left, functionName));
            }
            else if (n.left == null && n.right != null && functionName == n.data)
            {
                paramList.Add(sys.Assign(n.parent.right.data));
            }
            if (n.right != null)
            {
                paramList.AddRange(GetParams(n.right,functionName));
            }

            return paramList;
        }

      
           


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


        public void ExecOp(TreeNode n)
        {
            NetLogoObject obj = null;
            ParseTree t = new ParseTree("");
            if (nodeFull(n))
            {
                if (t.IsOperator(n.left))
                {
                    ExecOp(n.left);

                }
                if (t.IsOperator(n.right))
                {
                    ExecOp(n.left);

                }
                obj = OperatorTable.Call<NetLogoObject>(sys.Assign(n.left.data, false), sys.Assign(n.right.data, false), n.data);
            }
            else if (n.left != null)
            {
                obj = OperatorTable.Call<NetLogoObject>(sys.Assign(n.left.data, false), new NetLogoObject() { ptrID = "NULLPTR" }, n.data);
            }
            else
            {
                obj = OperatorTable.Call<NetLogoObject>(new NetLogoObject() { ptrID = "NULLPTR" }, new NetLogoObject() { ptrID = "NULLPTR" }, n.data);
            }
            TreeNode tempNode = new TreeNode(obj.value.ToString());
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

        private bool isOp(TreeNode n)
        {
            ParseTree tr = new ParseTree("");
            return tr.IsOperator(n);
        }

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

