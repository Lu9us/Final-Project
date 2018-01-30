using System;
using System.IO;
using System.Linq;

namespace NParser
{
    //TODO: parse post line comments 
    public class Parser
    {
        int PC = 0;
        public bool fileEnd = false;
        string[] data;
        public bool skipBadLines = false;
        string[] declarativeKeywords = { "extensions", "breed", "globals" };
        string[] agentDeclarativeKeywords = { "-own" };
        string[] functionDeclarativeKeywords = { "to" };
        public void LoadFile(string fileName)
        {
            data = File.ReadAllLines(fileName);

        }

        public void Read()
        {
            if (data != null && PC < data.Length)
            {
                string line = data[PC];
                line = line.TrimStart();
                string FirstStatment = "";
                try { FirstStatment = line.Substring(0, line.IndexOf(' ')); }
                catch (Exception e) { Console.WriteLine("short line"); };
                Console.WriteLine(line);
                Console.WriteLine("first statement: " + FirstStatment);
                if (line.StartsWith(";"))
                {
                    Console.WriteLine("Line Comment skipping");
                }
                else if (declarativeKeywords.Any(a => line.StartsWith(a)))
                {
                    Console.WriteLine("Declartive statment");
                    Declarative(line);

                }
                else if (agentDeclarativeKeywords.Any(a => FirstStatment.Contains(a)))
                {
                    Console.WriteLine("Agent Declartive statment");
                    AgentDeclaritive(line);

                }
                else if (functionDeclarativeKeywords.Any(a => FirstStatment.Contains(a)))
                {
                    Console.WriteLine("Function Declartive statment");
                    FunctionDeclarative(line);

                }
                PC++;
            }
            else if (PC >= data.Length)
            {
                fileEnd = true;
            }
        }

        public void AgentDeclaritive(string line)
        {

        }

        public void Declarative(string line)
        {

        }
        public void FunctionDeclarative(string line)
        {

        }

    }
}
