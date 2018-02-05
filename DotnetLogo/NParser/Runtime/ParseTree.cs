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
        string[] operators = new[] { "+", "-", "*", "/","set","let","ask" };
        TreeNode root;
        ParseTree(string expression)
        {
            root = new TreeNode(expression);
            string[] tokens = StringUtilities.split(delims, expression);

            Stack<string> tokenStack = new Stack<string>();
            Stack<string> opearatorStack = new Stack<string>();

            foreach (string data in tokens)
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
    }
}
