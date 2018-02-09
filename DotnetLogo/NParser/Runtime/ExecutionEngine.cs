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
        public Parser p;

        public ExecutionEngine()
        {

            p = new Parser(sys);
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
            else if (sys.checkType(n.data) == typeof(NetLogoObject))
            {
                try
                {
                    ExecFunction(n);
                }
                catch (Exception e)
                {
                    // GetVar(n);
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

            StackFrame fFrame = new StackFrame(f.name, new Dictionary<string, NetLogoObject>());

            fFrame.pc = 0;
            sys.exeStack.Push(fFrame);
            ParseTree pt;
            while (fFrame.pc < f.body.Length)
            {
                pt = new ParseTree(f.body[fFrame.pc]);
                ExecuteTree(pt);
               fFrame.pc++;
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
                paramList.AddRange(GetParams(n.left,functionName));
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

            //switch (n.data)
            //{
            //    case "+":
            //        _add(n);
            //        break;
            //    case "let":
            //        if (nodeFull(n))
            //        {
            //            if (sys.checkType(n.left.data) == typeof(NetLogoObject))
            //            {
            //                sys.exeStack.Peek().locals.Add(n.left.data, sys.Assign(n.right.data));

            //            }
            //        }
            //        break;
            //    case "set":
            //        if (nodeFull(n))
            //        {
            //            if (sys.checkType(n.left.data) == typeof(NetLogoObject))
            //            {
            //                sys.exeStack.Peek().locals[n.left.data]= sys.Assign(n.right.data);

            //            }
            //        }
            //        break;
            //    case "report":
            //        if (n.left != null)
            //        {
            //            if (sys.checkType(n.left.data) == typeof(NetLogoObject))
            //            {
            //                Console.WriteLine(sys.GetVal(sys.Get(n.left.data)));
            //            }
            //            else
            //            {
            //                Console.WriteLine(n.left.data);
            //            }

            //        }
            //        break;
            //    default:
            //        break;

            //}

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

        private void _add(TreeNode n)
        {
            if (nodeFull(n))
            {
                if (sys.checkType(n.left.data) == typeof(Number) && sys.checkType(n.right.data) == typeof(Number))
                {


                    __add(float.Parse(n.left.data), float.Parse(n.right.data),n) ;

                   
                   
                }
                else if (sys.checkType(n.left.data) == typeof(NetLogoObject) && sys.checkType(n.right.data) == typeof(NetLogoObject))
                {
                    NetLogoObject o1 = (sys.Get(n.left.data));
                    NetLogoObject o2 = (sys.Get(n.right.data));
                    if (o1 is Number && o2 is Number)
                    {
                        __add(((Number)o1).val, ((Number)o2).val, n);
                    }
                }
                else if (sys.checkType(n.left.data) == typeof(Number) && sys.checkType(n.right.data) == typeof(NetLogoObject))
                {

                }
                else if (sys.checkType(n.left.data) == typeof(NetLogoObject) && sys.checkType(n.right.data) == typeof(Number))
                {

                }
            }


        }
        private void __add(float a, float b,TreeNode n)
        {
            float result = a + b;

            TreeNode temp = n.parent;
            TreeNode node = new TreeNode(result.ToString());
            if (n.parent.left == n)
            {
                n.parent.left = node;
            }
            else if (n.parent.right == n)
            {
                n.parent.right = node;
            }
            node.parent = n.parent;
        }

    }

     
    }

