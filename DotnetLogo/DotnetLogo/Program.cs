using NParser;
using NParser.Runtime;
using NParser.Runtime.DataStructs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetLogo
{
    class Program
    {
        static ExecutionEngine engine = new ExecutionEngine();
        static void Main(string[] args)
        {
            string s = "";
            while (s != "exit")
            {
                Console.Write("$: ");
                s = Console.ReadLine();

                if (s.ToLower() == "test")
                {

                    engine.Load("Testing/Test.nlogo");

                }
                else
                {
                  
                    ParseTree p = new ParseTree(s);
                    engine.ExecuteTree(p);



                }
            }
        }
    }
}


       

