using NParser.Runtime.DataStructs;
using NParser.Types;
using NParser.Types.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Runtime
{
    public class ExecutionEngine
    {
        public SystemState sys = new SystemState();
        public PreProcessor p;
      
        public ExecutionEngine()
        {

            p = new PreProcessor(sys);
            StackFrame t = new StackFrame("INT", new Dictionary<string, NetLogoObject>());
            sys.exeStack.Push(t);
        }

        public void Load(string path)
        {
            p.LoadFile(path);
            while (!p.fileEnd)
                p.FirstPassRead();

        }

        public void ExecuteTree(ParseTree t)
        {
            TreeNode n = ReadToLeaf(t.root);
            Exec(n, t);



        }

        private void Exec(TreeNode n, ParseTree t)
        {

            if (n == t.root)
            {
                return;
            }
            if (t.IsOperator(n))
            {
                ExecOp(n);

            }
            else if (sys.checkType(n.data) == typeof(NetLogoObject)&& sys.registeredFunctions.ContainsKey(n.data))
            {
                try
                {
                    ExecFunction(n);
                }
                catch (Exception e)
                {
                    //throw new RTException("Exception occured during execution: " + e.Message);
                }


            }
            Exec(n.parent, t);
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

        public void ExecFunction(TreeNode n)
        {
            Function f = sys.registeredFunctions[n.data];

            if (sys.registeredFunctions.ContainsKey(n.left.data) && n.right != null)
            {
                TreeNode ns = ReadToLeaf(n.left);
                ns.left = new TreeNode(n.right.data);
                ns.left.parent = ns;
                n.right = null;
                ExecFunction(n.left);

            }
            else if (sys.registeredFunctions.ContainsKey(n.left.data))
            {
                ExecFunction(n.left);
            }
            sys.BreakExecution = false;
            List<NetLogoObject> vals = GetParams(n, f.name);
            
            Dictionary<string, NetLogoObject> objects = new Dictionary<string, NetLogoObject>();
            for (int i = 0; i < f.paramaters.Count; i++)
            {
                objects.Add(f.paramaters[i], vals[i]);
            }
            StackFrame fFrame = new StackFrame(f.name, objects) { Report = f.Report };

            fFrame.pc = 0;
            sys.exeStack.Push(fFrame);
            ParseTree pt;
            while (fFrame.pc < f.body.Length && !sys.BreakExecution)
            {
                pt = new ParseTree(f.body[fFrame.pc]);
                


                ExecuteTree(pt);
               fFrame.pc++;
            }
#if DEBUG 
            Console.WriteLine("Execution broken : "+ sys.BreakExecution);
#endif 

            if (f.Report && sys.exeStack.Peek().ReportValue != null)
            {
                sys.BreakExecution = false;
                TreeNode tempNode = new TreeNode(sys.Assign(sys.exeStack.Peek().ReportValue.value.ToString()).value.ToString());
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


          StackFrame oldFrame = sys.exeStack.Pop();
#if DEBUG
         Console.WriteLine(oldFrame.ToString());
#endif
        }
        private List<NetLogoObject> GetParams(TreeNode n,string functionName)
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
            else if (n.left == null && functionName == n.data)
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
            if (nodeFull(n))
            {
                obj = OperatorTable.Call<NetLogoObject>(sys.Assign(n.left.data,false), sys.Assign(n.right.data,false), n.data);
            }
            else
            {
                obj = OperatorTable.Call<NetLogoObject>(sys.Assign(n.left.data,false), new NetLogoObject() {ptrID = "NULLPTR" },n.data);
            }
            TreeNode tempNode = new TreeNode(obj.value.ToString());
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

