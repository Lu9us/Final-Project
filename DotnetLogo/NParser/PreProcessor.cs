using NParser.Runtime;
using NParser.Types;
using NParser.Types.Agents;
using NParser.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NParser.Utils;
using static NParser.Runtime.FlowControll;

namespace NParser
{
/// <summary>
///Pre-proccessor class runs before the main parser and sepereates out
///flow controll and functions
/// </summary>
    public class PreProcessor
    {


        SystemState s;
        
        //program counter
        int PC = 0;
       
        public bool fileEnd = false;
        public bool skipBadLines = false;

        string[] data;
        string[] declarativeKeywords = { "extensions", "breed", "globals" };
        string[] agentDeclarativeKeywords = { "-own" };
        string[] functionDeclarativeKeywords = { "to" };
        string[] flowControllKeywords = { "if","elseif"};

        string askKeyword = "ask";
        string createKeyword = "create-";

        char[] delims = new[] { ' ', '[', ']', ',' };


        public PreProcessor(SystemState st)
        {
            s = st;
        }
        /// <summary>
        /// set the internal line buffer for the pre processor
        /// </summary>
        /// <param name="data">lines of code</param>
        internal void SetData(string[] data)
        {
            this.data = data;
            PC = 0;
            for (int i = 0; i < data.Length; i++)
            {
               data[i] = data[i].Replace('\n', ' ');
                data[i] = data[i].Trim();
            }
        }
        /// <summary>
        /// Load a file into the line buffer buffer 
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadFile(string fileName)
        {
            try
            {
                data = File.ReadAllLines(fileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("file not found");
            }
        }
        /// <summary>
        /// back bone function for the preprocessor 
        /// </summary>
        public void FirstPassRead()
        {
            if (data != null && PC < data.Length)
            {
                //get rid of any stray new line chars
                string line = data[PC].Replace('\n',' ');
                line = line.TrimStart();
                string FirstStatment = "";

                try
                {
                    FirstStatment = line.Substring(0, line.IndexOf(' '));
                }
                catch (Exception e)
                { //Console.WriteLine("short line"); 
                };

                
                try
                {
                    // maybe switch ?
                    if (line.StartsWith(";"))
                    {
                     
                    }
                    else if (declarativeKeywords.Any(a => line.StartsWith(a)))
                    {
                       
                       // Declarative(line);

                    }
                    else if (agentDeclarativeKeywords.Any(a => FirstStatment.Contains(a)))
                    {
                        
                        AgentDeclaritive(line);

                    }
                    else if (functionDeclarativeKeywords.Any(a => FirstStatment.StartsWith(a)))
                    {
                        
                        FunctionDeclarative(line);

                    }
                 
                    PC++;
                }
                catch (RTException e)
                {


                    Console.WriteLine("Parsing failed Exception Details: " + e.ToString());
                   
                }
            }
            else if (PC >= data.Length)
            {
                fileEnd = true;
            }
        }
        /// <summary>
        /// construct breeds and add additional paramaters to existing breeds
        /// </summary>
        /// <param name="line"></param>
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
        /// <summary>
        /// Constructs a function to create agents 
        /// </summary>
        /// <param name="line">start line</param>
        /// <param name="tpc">program counter</param>
        /// <param name="offset">overall offset</param>
        /// <returns></returns>
        public AgentCreationStatement AgentCreate(string line, int tpc,int offset)
        {

            int tempPC = tpc;
            int spc = tpc;

            string token = "";

            try
            {
                string[] lineData = StringUtilities.split(new[] { '-',' ' }, this.data[spc]);

                List<FlowControll> fc = new List<FlowControll>();

                string name = lineData[2];
                string count = lineData[3];

                Stack<string> stack = new Stack<string>();


                do
                {
                    lineData = StringUtilities.split(delims, this.data[tempPC]);

                    foreach (string td in lineData)
                    {



                        if (flowControllKeywords.Any(a => td.Equals(a)))
                        {
                            fc.Add(FlowControl(tempPC - 1, this.data[tempPC - 1]));
                        }

                        if (td == "]")
                        {
                            stack.Pop();
                        }
                        else if (td == "[")
                        {
                            stack.Push("[");
                        }
                    }
                    tempPC++;
                    //token = this.data[tempPC];

                }

                while (stack.Count > 0);
                string[] lines = new string[tempPC - 1 - (spc + 1)];
                for (int i = spc + 1; i < tempPC - 1; i++)
                {
                    lines[i - (spc + 1)] = this.data[i];

                }

                AgentCreationStatement statement = new AgentCreationStatement(lines,offset, "NEWAGENT " + name) {breed=name,countVar = count };
               
                return statement;
            }
            catch (Exception e)
            {
#if DEBUG
                Debugger.Break();
#endif
                throw new RTException("Function parsing failed on line " + tempPC + " with exception " + e.Message);
            }


        }
        /// <summary>
        /// Constructs a function to track agent behaviour
        /// </summary>
        /// <param name="line">start line</param>
        /// <param name="tpc">program counter</param>
        /// <param name="offset">overall offset</param>
        /// <returns></returns>
        public Ask AskDeclarative(string line,int tpc,int offset)
        {

            int tempPC = tpc;
            int spc = tpc;

            string token = "";
            try
            {
                string[] lineData = StringUtilities.split(delims,line);

                List<FlowControll> fc = new List<FlowControll>();
                List<Ask> AskData = new List<Ask>();
                List<string> paramaters = new List<string>();

                bool report = false;
               
                string name = lineData[1];

                Stack<string> stack = new Stack<string>();
                

                do
                {
                    lineData = StringUtilities.split(delims, this.data[tempPC]);

                    foreach (string td in lineData)
                    {

                        if (flowControllKeywords.Any(a => td.Equals(a)))
                        {
                            FlowControll fcc = FlowControl(tempPC + 1, this.data[tempPC]);
                            foreach (Block b in fcc.JumpTable.Values)
                            {
                                tempPC += b.body.Length;
                            }
                            fc.Add(fcc);
                        }
                        if (td.Equals(askKeyword) && stack.Count > 0 && tempPC != tpc)
                        {
                            Ask ask = AskDeclarative(this.data[tempPC], tempPC, tempPC - tpc );
                          
                            AskData.Add(ask);
                        }

                        if (td == "]")
                        {
                            stack.Pop();
                        }
                        else if (td == "[")
                        {
                            stack.Push("[");
                        }
                    }
                    tempPC++;

                }
                while (stack.Count > 0);

                string[] lines = new string[tempPC - 1 - (spc + 1) ];

                for (int i = spc+1; i < tempPC-1; i++)
                {
                    lines[i - (spc + 1)] = this.data[i];

                }

                Ask f = new Ask(lines, spc , name) { pcOffset = offset, paramaters = paramaters, Report = report };
                f.askData = AskData;
                f.flowControls = fc;

                return f;
            }
            catch (Exception e)
            {
#if DEBUG
                Debugger.Break();
#endif
                fileEnd = true;
                throw new RTException("Function parsing failed on line " + tempPC + " with exception " + e.Message);
               
            }


        }
/// <summary>
/// creates a function data structure
/// </summary>
/// <param name="line">Inital Line</param>
        public void FunctionDeclarative(string line)
        {
            int tempPC = PC + 1;
         
            string token = "";
            try
            {
                string[] lineData = StringUtilities.split(delims, this.data[PC]);

                List<FlowControll> fc = new List<FlowControll>();
                List<string> paramaters = new List<string>();
                List<Ask> ak = new List<Ask>();
                List<AgentCreationStatement> createAgents = new List<AgentCreationStatement>();

                bool report = false;
                bool param = false;

                string name = lineData[1];
                foreach (string td in lineData)
                {

                    if (lineData.Length > 1 && td == "[" && lineData[1] == name)
                    {
                        param = true;
                    }

                    if (td == "]")
                    {
                        param = false;
                    }

                    if (param && !(td == "[" || td == "]") && !string.IsNullOrEmpty(td))
                    {
                        paramaters.Add(td);
                    }
                }

                if (lineData[0] == "to-report")
                {
                    report = true;
                }
                while (token != "end")
                {
                    if (this.data[tempPC].IndexOfAny(delims) != -1)
                    {
                        lineData = StringUtilities.split(delims, this.data[tempPC]);
                    }
                    else
                    {
                        lineData = new[] { this.data[tempPC] };
                    }
                    foreach (string td in lineData)
                    {
                        
                      
                        if (flowControllKeywords.Any(a => td.Equals(a)))
                        {
                            FlowControll fcc = FlowControl(tempPC + 1, this.data[tempPC]);
                            foreach (Block b in fcc.JumpTable.Values)
                            {
                                tempPC += b.body.Length;
                            }
                            
                          fc.Add(fcc);
                        }
                        if (td.Equals(askKeyword))
                        {
                            ak.Add(AskDeclarative(this.data[tempPC],tempPC, tempPC - PC));
                        }
                        if (td.StartsWith(createKeyword))
                        {
                            createAgents.Add(AgentCreate(this.data[tempPC], tempPC, tempPC-PC));
                        }
                    }
                    
                    foreach (string s in lineData)
                    {
                        if (s == "end")
                        {
                            token = s;
                            break;
                        }
                    }
                    tempPC++;
                    //token = this.data[tempPC];

                }
                string[] lines = new string[tempPC - (PC+1)];
                for (int i = PC+1; i < tempPC; i++)
                {
                    lines[i - (PC+1)] = this.data[i];
                   
                }

                Function f = new Function(lines, PC + 1, name) { paramaters = paramaters, Report = report };
                f.askData = ak;
                f.flowControls = fc;
                f.agentData = createAgents;
                s.AddFunction(f);
            }
            catch (Exception e)
            { throw new RTException("Function parsing failed on line "+ tempPC + " with exception "+ e.Message); }


        }
        /// <summary>
        /// create a flow controll structure starting at tpc using line
        /// </summary>
        /// <param name="tPC"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public FlowControll FlowControl(int tPC,string line)
        {
            line = line.Trim();
            int tempPC = tPC;
            string type = line.Substring(0,line.IndexOf(' '));
            string fullLine = line.Split(new[] { '\n','['})[0];
            List<FlowControll> fc = new List<FlowControll>();
            List<Ask> ak = new List<Ask>();
            List<AgentCreationStatement> createAgents = new List<AgentCreationStatement>();
            Stack<string> expected = new Stack<string>();

            string[] splitTokens;
            string tempLine;

            expected.Push("]");

            JumpType jump = JumpType.Succes;

            FlowControll f = new FlowControll(type);

            int blockStart = tempPC;
            int blockEnd = 0;
            
            while (expected.Count > 0)
            {
                if (expected.Count > 0 && tempPC >= data.Length)
                {
                    throw new RTException("Expected " + expected.Peek() + "got eof");
                }

                tempLine = data[tempPC];
                splitTokens = StringUtilities.split(delims,tempLine);
                
                foreach (string token in splitTokens)
                {
                    if (!string.IsNullOrWhiteSpace(token))
                    {

                        if (flowControllKeywords.Any(a => token.Equals(a)) && tempPC != tPC)
                        {

                            FlowControll flow = FlowControl(tempPC, this.data[tempPC]);
                            foreach (Block b in flow.JumpTable.Values)
                            {
                                tempPC += b.body.Length;
                            }
                           
                            fc.Add(flow);
                        }
                        if (token.Equals(askKeyword))
                        {
                            var ask = AskDeclarative(this.data[tempPC], tempPC, tempPC - tPC);
                            tempPC += ask.body.Length;
                            ak.Add(ask);
                        }
                        if (token.StartsWith(createKeyword))
                        {
                            var agent = AgentCreate(this.data[tempPC], tempPC, tempPC - tPC);
                            tempPC += agent.body.Length;
                            createAgents.Add(agent);
                        }




                        if (token == "[" && expected.Peek() == "[" && expected.Count == 1)
                        {
                            expected.Pop();
                            expected.Push("]");
                            blockStart = tempPC;

                        }
                        else if (token == "[" && expected.Count >= 1)
                        {
                            expected.Push("]");
                        }
               

                        if (token == "]" && expected.Peek() == "]" && expected.Count == 1)
                        {
                            blockEnd = tempPC;
                            expected.Pop();
                         
                            List<string> lines = new List<string>();
                            for (int i = blockStart ; i < blockEnd; i++)
                            {

                                lines.Add(data[i]);

                            }

                            Block b = new Block(blockStart-(PC+1),lines.ToArray(),jump);
                            b.flowControls = fc;
                            b.askData = ak;
                            b.agentData = createAgents;
                            b.end = blockEnd;
                            f.JumpTable.Add(jump, b);
                            f.conditionalLine = fullLine;
                            if (type == "elseif" && jump == JumpType.Succes)
                            {
                                expected.Push("[");
                                jump = JumpType.Fail;

                            }
                            break;

                        }
                        else if (token == "]" && expected.Peek() == "]" && expected.Count > 1)
                        {
                            expected.Pop();
                        }
                    }

                }
                tempPC++;
              
            }

            return f;

        }
    }
}
