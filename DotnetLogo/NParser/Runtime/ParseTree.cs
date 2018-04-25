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

        internal TreeNode(TreeNode node, TreeNode parent)
        {
            this.data = node.data;
            this.parent = parent;
            if (node.left != null)
            {
                this.left = new TreeNode(node.left,this);
            }
            if (node.right != null)
            {
                this.right = new TreeNode(node.right, this);
            }
        }


        public TreeNode parent;
        public readonly string data;
        public TreeNode left;
        public TreeNode right;
    }

    public class ParseTree
    {
        internal static Dictionary<string,ParseTree> treeCache = new Dictionary<string, ParseTree>();
        char[] delims = new[] { ' ', '[', ']', ',' };
        List<string> operators = OperatorTable.opTable.Select(o => o.Key.token ).ToList();
      
        public  TreeNode root;

        private ParseTree(TreeNode root)
        {
            this.root = new TreeNode(root, null);
        }

        public ParseTree(string expression)
        {
            if (treeCache.ContainsKey(expression))
            {
                root = new TreeNode(treeCache[expression].root, null);
                return;
            }

            root = new TreeNode(expression);
            string[] tokens = StringUtilities.split(delims, expression);
            if (expression.Trim().StartsWith(";"))
            {
                this.root = new TreeNode(expression);
                if (!treeCache.ContainsKey(expression))
                {
                    treeCache.Add(expression,new ParseTree( this.root));
                }
                return;

            }
            Stack<string> tokenStack = new Stack<string>();
            Stack<string> opearatorStack = new Stack<string>();



            foreach (string data in tokens.Reverse())
            {

                if (!string.IsNullOrWhiteSpace(data) && !delims.Contains(data.ToCharArray()[0]))
                {
                    if (operators.Contains(data) && !data.Equals("random"))
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

            this.root.left = NodeGen(tokenStack, opearatorStack, this.root);
            if (!treeCache.ContainsKey(expression))
            {
                treeCache.Add(expression, new ParseTree(this.root));
            }
        }


        private TreeNode NodeGen( Stack<string> tokenStack, Stack<string> opearatorStack,TreeNode parent)
        {
           
            TreeNode tempNode = null;
            if (tokenStack.Count > 0 && opearatorStack.Count > 0)
            {
                tempNode = new TreeNode(opearatorStack.Pop());
                tempNode.parent = parent;

                if (tokenStack.Count > 0&&opearatorStack.Count<1)
                {
                    tempNode.left = new TreeNode(tokenStack.Pop());
                    tempNode.left.parent = tempNode;
                }
                if (opearatorStack.Count > 0)
                {
                    tempNode.left = new TreeNode(opearatorStack.Pop());
                    tempNode.left.parent = tempNode;
                    if (opearatorStack.Count < 1)
                    {
                        if (tokenStack.Count == 3)
                        {

                            tempNode.right = new TreeNode(tokenStack.Pop());
                            tempNode.right.parent = tempNode;
                        }

                        if (tempNode.left.left == null&& tokenStack.Count > 0)
                        {

                            tempNode.left.left = new TreeNode(tokenStack.Pop());
                            tempNode.left.left.parent = tempNode.left;
                        }
                        if (tokenStack.Count > 0&& tempNode.left.right == null)
                        {
                         
                            tempNode.left.right = new TreeNode(tokenStack.Pop());
                            tempNode.left.right.parent = tempNode.left;
                        }
                        if (tokenStack.Count >0)
                        {

                            tempNode.right = new TreeNode(tokenStack.Pop());
                            tempNode.right.parent = tempNode;
                        }
                    }
                    else
                    {
                        if (tokenStack.Count > 0)
                        {
                           
                            tempNode.left.right = new TreeNode(tokenStack.Pop());
                            tempNode.left.right.parent = tempNode.left;
                        }
                        tempNode.left.left = NodeGen(tokenStack, opearatorStack, tempNode.left);
                       
                    }

                }

            }
            else if (tokenStack.Count > 0)
            {
                tempNode = new TreeNode(tokenStack.Pop());
                tempNode.parent = parent;
                if (tokenStack.Count > 0&&tempNode.left == null)
                {
                    tempNode.left = new TreeNode(tokenStack.Pop());
                    tempNode.left.parent = tempNode;
                }
            
            }
            else if (opearatorStack.Count > 0 && tokenStack.Count < 1)
            {
                tempNode = new TreeNode(opearatorStack.Pop());
                tempNode.parent = parent;
                if (parent.parent != null&& parent != null&& parent.parent != root && parent.parent.right == null)
                {
                    
                    tempNode.parent.parent.right = tempNode;
                    return null;
                }
                else
                {
                    tempNode.parent.left = tempNode;
                }

            }
            if (opearatorStack.Count > 0)
            {
                tempNode.right = NodeGen(tokenStack, opearatorStack, tempNode);
            }
            else if (tokenStack.Count > 0 && opearatorStack.Count == 0)
            {
                if (IsOperator(tempNode.left))
                {
                    tempNode.left.right = TokenNodeGen(tokenStack, tempNode);
                }
                else
                {
                    tempNode.right = TokenNodeGen(tokenStack, tempNode);
                }
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
            if (n != null)
            {
                return operators.Contains(n.data);
            }
            else
            {
                return false;
            }
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

