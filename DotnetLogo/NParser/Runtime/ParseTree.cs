using NParser.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NParser.Types.Internals;

namespace NParser.Runtime.DataStructs
{
    public class TreeNode
    {
        internal TreeNode(string data)
        {
            this.data = data;
        }
        public TreeNode parent;
        public readonly string data;
        public TreeNode left;
        public TreeNode right;
    }
    public class ParseTree
    {
        char[] delims = new[] { ' ', '[', ']', ',' };
        string[] operators = OperatorTable.opTable.Select(o => o.Key.token).ToArray();
        public readonly TreeNode root;
      public  ParseTree(string expression)
       {
            root = new TreeNode(expression);
            string[] tokens = StringUtilities.split(delims, expression);

            Stack<string> tokenStack = new Stack<string>();
            Stack<string> opearatorStack = new Stack<string>();

          

            foreach (string data in tokens.Reverse())
            {
                if (!string.IsNullOrWhiteSpace(data))
                {
                    if (operators.Contains(data))
                    {



                        opearatorStack.Push(data);

                    }

                    else
                    {
                        tokenStack.Push(data);
                    }


                }
            }
            if (tokenStack.Count == 0 && opearatorStack.Count == 0)
            {
                return;
            }

            this.root.left = NodeGen( tokenStack, opearatorStack,this.root);

        }


        private TreeNode NodeGen( Stack<string> tokenStack, Stack<string> opearatorStack,TreeNode parent)
        {
            TreeNode tempNode = null;
            if (opearatorStack.Count > 0)
            {
                tempNode = new TreeNode(opearatorStack.Pop());
                tempNode.parent = parent;
                tempNode.left = new TreeNode(tokenStack.Pop());
                tempNode.left.parent = tempNode;
            }
            else if (tokenStack.Count > 0)
            {
                tempNode = new TreeNode(tokenStack.Pop());
                tempNode.parent = parent;
                if (tokenStack.Count > 0)
                {
                    tempNode.left = new TreeNode(tokenStack.Pop());
                    tempNode.left.parent = tempNode;
                }
            
            }
            if (opearatorStack.Count > 0)
            {
                tempNode.right = NodeGen(tokenStack, opearatorStack, tempNode);
            }
            else if (tokenStack.Count > 0 && opearatorStack.Count == 0)
            {
                tempNode.right = TokenNodeGen(tokenStack, tempNode);
            }
            else if (tokenStack.Count > 0)
            {
                tempNode.right = new TreeNode(tokenStack.Pop()); ;
                tempNode.right.parent = tempNode;
            }
          
           
            return tempNode;
            
        }

        private TreeNode TokenNodeGen(Stack<string> tokenStack, TreeNode parent)
        {
            TreeNode tempNode = new TreeNode(tokenStack.Pop());
            tempNode.parent = parent;
            if (tokenStack.Count > 0)
            {
                tempNode.left = TokenNodeGen(tokenStack, tempNode);
            }
            if (tokenStack.Count > 0)
            {
                tempNode.right = TokenNodeGen(tokenStack, tempNode);
            }

            return tempNode;
        }

        public bool IsOperator(TreeNode n)
        {
            return operators.Contains(n.data);
        }

        public  void printTree(TreeNode n, int level, int x)
        {
            Console.WriteLine(new string(' ', 40 - x - level) + n.data);
            level++;
            if (n.left != null)
            {
                printTree(n.left, level, 10);
            }
            if (n.right != null)
            {
                printTree(n.right, level, 5);
            }

            if (n.right == null && n.left == null)
            {
                return;
            }
        }
    }

}

