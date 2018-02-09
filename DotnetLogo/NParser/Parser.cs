using NParser.Runtime;
using NParser.Types;
using NParser.Types.Agents;
using NParser.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static NParser.Runtime.FlowControll;

namespace NParser
{
    //TODO: parse post line comments 
    public class PreProcessor
    {


        SystemState s;
        int PC = 0;
        public bool fileEnd = false;
        string[] data;
        public bool skipBadLines = false;
        string[] declarativeKeywords = { "extensions", "breed", "globals" };
        string[] agentDeclarativeKeywords = { "-own" };
        string[] functionDeclarativeKeywords = { "to" };
        string[] flowControllKeywords = { "if","elseif"};
          char[] delims = new[] { ' ', '[', ']', ',' };


        public PreProcessor(SystemState st)
        {
            s = st;
        }
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
                 
                    PC++;
                }
                catch (RTException e)
                {


                    Console.WriteLine("Parsing failed Exception Details: " + e.ToString());
                    if (!skipBadLines)
                    { 
#if DEBUG
                        Debugger.Break();
#endif
                    }
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
            int tempPC = PC + 1;
         
            string token = "";
            try
            {
                string[] lineData = StringUtilities.split(delims, this.data[PC]);
                List<FlowControll> fc = new List<FlowControll>();
                List<string> paramaters = new List<string>();
                bool param = false;
                string name = lineData[1];
                while (token != "end")
                {
                    foreach (string td in lineData)
                    {

                        if (td == "[" && lineData[1] == name)
                        {
                            param = true;
                        }
                        if (td == "]")
                        {
                            param = false;
                        }
                        if (param && !(td == "[" || td== "]")&& !string.IsNullOrEmpty(td))
                        { paramaters.Add(td); }
                        if (flowControllKeywords.Any(a => td.Equals(a)))
                        {
                          fc.Add(FlowControl(tempPC-1, this.data[tempPC-1]));
                        }
                    }
                    lineData = StringUtilities.split(delims, this.data[tempPC]);
                    foreach (string s in lineData)
                    {
                        if (s == "end")
                        {
                            token = s;
                            break;
                        }
                    }
                    tempPC++;
                    token = this.data[tempPC];

                }
                string[] lines = new string[tempPC - (PC+1)];
                for (int i = PC+1; i < tempPC; i++)
                {
                    lines[i - (PC+1)] = this.data[i];
                   
                }
                
                Function f = new Function(lines, PC + 1, name) {paramaters = paramaters };
                f.flowControls = fc;
                s.AddFunction(f);
            }
            catch (Exception e)
            { throw new RTException("Function parsing failed"); }


        }

        public FlowControll FlowControl(int pc,string line)
        {
            line = line.Trim();
            string type = line.Substring(0,line.IndexOf(' '));
            string fullLine = line.Split(new[] { '\n','['})[0];

            Stack<string> expected = new Stack<string>();

            string[] splitTokens;
            string tempLine;

            expected.Push("[");

            JumpType jump = JumpType.Succes;

            FlowControll f = new FlowControll(type);

            int blockStart = 0;
            int blockEnd = 0;
            
            while (expected.Count > 0)
            {
                if (expected.Count > 0 && pc >= data.Length)
                {
                    throw new RTException("Expected " + expected.Peek() + "got eof");
                }

                tempLine = data[pc];
                splitTokens = StringUtilities.split(delims,tempLine);
                
                foreach (string token in splitTokens)
                {
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        if (token == "[" && expected.Peek() == "[" && expected.Count == 1)
                        {
                            expected.Pop();
                            expected.Push("]");
                            blockStart = pc;

                        }
                        else if (token == "[" && expected.Count > 1)
                        {
                            expected.Push("]");
                        }
               

                        if (token == "]" && expected.Peek() == "]" && expected.Count == 1)
                        {
                            blockEnd = pc;
                            expected.Pop();
                            if (type == "ifelse" && jump == JumpType.Succes)
                            {
                                expected.Push("[");
                                jump = JumpType.Fail;

                            }
                            Block b = new Block();
                            b.start = blockStart;
                            b.end = blockEnd;
                            f.JumpTable.Add(jump, b);
                            f.conditionalLine = fullLine;
                            break;

                        }
                        else if (token == "]" && expected.Peek() == "]" && expected.Count > 1)
                        {
                            expected.Pop();
                        }
                    }

                }
                pc++;
              
            }

            return f;

        }
    }
}
