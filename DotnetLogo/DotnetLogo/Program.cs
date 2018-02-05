using NParser;
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
        static void Main(string[] args)
        {
            string s = "";
            while (s != "exit")
            {
                Console.Write("$: ");
                s = Console.ReadLine();

                if (s.ToLower() == "test")
                {
                    Parser p = new Parser();
                    p.LoadFile("Testing\\Test.nlogo");
                    while (!p.fileEnd)
                    {
                        p.FirstPassRead();
                    }
                }
                else if (s.Contains( "treeGen "))
                {
                    string line = s.Substring(s.IndexOf(" ")+1);
                    ParseTree p = new ParseTree(line);
                }
            }
        }
    }
}
