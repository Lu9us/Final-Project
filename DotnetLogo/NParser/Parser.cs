using NParser.Types;
using NParser.Types.Agents;
using NParser.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NParser
{
    //TODO: parse post line comments 
    public class Parser
    {
        SystemState s = new SystemState();
        int PC = 0;
        public bool fileEnd = false;
        string[] data;
        public bool skipBadLines = false;
        string[] declarativeKeywords = { "extensions", "breed", "globals" };
        string[] agentDeclarativeKeywords = { "-own" };
        string[] functionDeclarativeKeywords = { "to" };
        string[] flowControllKeywords = { "if" };
        public void LoadFile(string fileName)
        {
            data = File.ReadAllLines(fileName);

        }

        public void FirstPassRead()
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
                try
                {
                    // maybe switch ?
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
                    else if (flowControllKeywords.Any(a => FirstStatment.Contains(a)))
                    {
                        Console.WriteLine("Controll flow statment");
                        FlowControl(line);
                    }
                    PC++;
                }
                catch (RTException e)
                {
                    Console.WriteLine("Parsing failed Exception Details: " + e.ToString());
#if DEBUG
                    Debugger.Break();
#endif

                }
            }
            else if (PC >= data.Length)
            {
                fileEnd = true;
            }
        }

        public void AgentDeclaritive(string line)
        {
            try
            {

                char[] delims = new[] { ' ', '[', ']', ',' };
                string[] tokens = StringUtilities.split(delims,line);
               
                Stack<string> scope = new Stack<string>();
                string name = tokens[0].Substring(0,tokens[0].IndexOf('-'));



                for (int i = 1; i < tokens.Length; i++)
                {
                    if (tokens[i] == "[")
                    {
                        scope.Push("[");
                    }
                    else if (tokens[i] == "]")
                    {
                        if (scope.Count < 1 || scope.Peek() != "[")
                        {
                            throw new RTException("Statment missing opening bracket");
                        }
                        scope.Pop();
                    }
                    else if (scope.Count > 0)
                    {
                        if (name == "patches")
                        {
                            foreach (Patch p in s.patches)
                            {
                                p.properties.AddProperty(tokens[i], new NetLogoObject());
                            }
                        }
                    }
                }
                    if (scope.Count() > 0)
                    { throw new RTException("Statment missing closing bracket"); }
                

            }
            catch (Exception e)
            {
                throw new RTException("malformed expression on line: " +PC);
            }
        }

        public void Declarative(string line)
        {

        }
        public void FunctionDeclarative(string line)
        {

        }
        public void FlowControl(string line)
        {


        }
    }
}
