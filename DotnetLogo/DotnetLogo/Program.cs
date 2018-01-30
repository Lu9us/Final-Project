using NParser;
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
            Parser p = new Parser();
            p.LoadFile("Testing\\Test.nlogo");
            while (!p.fileEnd)
            {
                p.FirstPassRead();
            }
        }
    }
}
