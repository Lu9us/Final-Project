using NParser.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
        string[] operators = new[] { "(","^", "/", "*","+", "-",")","set","let","ask" };
        TreeNode root;
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
         this.root.left = NodeGen( tokenStack, opearatorStack,this.root);

        }


        private TreeNode NodeGen( Stack<string> tokenStack, Stack<string> opearatorStack,TreeNode parent)
        {
          
          TreeNode tempNode =new TreeNode(opearatorStack.Pop());
            tempNode.parent = parent;
            tempNode.left = new TreeNode(tokenStack.Pop());
            tempNode.left.parent = tempNode;
            if (opearatorStack.Count > 0)
            {
                tempNode.right =NodeGen(tokenStack, opearatorStack, tempNode);
            }
            else if (tokenStack.Count > 0)
            {
                tempNode.right = new TreeNode(tokenStack.Pop()); ;
                tempNode.right.parent = root;
            }
           
            return tempNode;
            
        }
      
    }
}
