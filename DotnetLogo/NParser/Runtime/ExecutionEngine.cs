using NParser.Runtime.DataStructs;
using NParser.Types;
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

        private void Exec(TreeNode n,ParseTree t)
        {

            if (n == t.root)
            {
                return;
            }
            if (t.IsOperator(n))
            {
                ExecOp(n);

            }
            else if(checkType(n.data)== typeof(NetLogoObject))
            {
                try
                {
                    ExecFunction(n);
                }
                catch (Exception e)
                {
                    GetVar(n);
                }


            }
            Exec(n.parent,t);
        }

        public void GetVar(TreeNode n)
        {
            TreeNode temp = new TreeNode(GetVal(Get(n.data)));
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
            switch (n.data)
            {
                case "+":
                    if (nodeFull(n))
                    {
                        if (checkType(n.left.data) == typeof(Number) && checkType(n.right.data) == typeof(Number))
                        {
                            float result = float.Parse(n.left.data) + float.Parse(n.right.data);

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
                    break;
                case "let":
                    if (nodeFull(n))
                    {
                        if (checkType(n.left.data) == typeof(NetLogoObject))
                        {
                            sys.exeStack.Peek().locals.Add(n.left.data, Assign(n.right.data));

                        }
                    }
                    break;
                case "set":
                    if (nodeFull(n))
                    {
                        if (checkType(n.left.data) == typeof(NetLogoObject))
                        {
                            sys.exeStack.Peek().locals[n.left.data]=  Assign(n.right.data);

                        }
                    }
                    break;
                case "report":
                    if (n.left != null)
                    {
                        if (checkType(n.left.data) == typeof(NetLogoObject))
                        {
                            Console.WriteLine(GetVal(Get(n.left.data)));
                        }
                        else
                        {
                            Console.WriteLine(n.left.data);
                        }

                    }
                    break;
                default:
                    break;

            }

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

        public NetLogoObject Assign(string s)
        {
            if (char.IsDigit(s[0]))
            {
                return new Number() { val = float.Parse(s) };
            }
            else if (char.IsLetter(s[0]))
            {
                Assign(GetVal(Get(s)));
            }
            else if (s[0] == '"')
            { return new NSString() { val = s }; }

            return null;

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
            if (sys.exeStack.Peek().locals.ContainsKey(s))
            {
                return sys.exeStack.Peek().locals[s];
            }
           else if (sys.globals.ContainsKey(s))
            {
                return sys.globals[s];
            }
            return null;
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

