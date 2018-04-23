using NParser.Runtime;
using NParser.Runtime.DataStructs;
using NParser.Types;
using NParser.Types.Agents;
using NParser.Performance;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetMASS
{
    public partial class Form1 : Form
    {
        private ExecutionEngine es = new ExecutionEngine();
        private Dictionary<string, Bitmap> spriteMap = new Dictionary<string, Bitmap>();
        private Bitmap data;
        private string[,] colorMap;
        private bool exeThread = false;
        bool scriptVerified = false;
        Thread t;
        public Form1()
        {
            InitializeComponent();
            colorMap = new string[50, 50];
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtScript.ScrollBars = ScrollBars.Vertical;
            txtConsole.ScrollBars = ScrollBars.Vertical;
            pbSim.Paint += AgentDraw;
            pbSim.Size = new Size(600, 600);
            spriteMap.Add("Agent", new Bitmap(Image.FromFile("Images/Agent.png")));
            spriteMap.Add("Tile", new Bitmap(Image.FromFile("Images/Tile.png")));
            Console.SetOut(new ConsoleToControl(txtConsole));

        }

        private Bitmap changeColour(Bitmap src, Color c,string agentType)
        {
            Color actualC;
            Bitmap b;
            if (!spriteMap.ContainsKey(agentType+c.Name))
            {
                 b = new Bitmap(src.Width, src.Height);
                for (int i = 0; i < src.Width; i++)
                {
                    for (int j = 0; j < src.Height; j++)
                    {
                        actualC = src.GetPixel(i, j);
                        if (actualC.A > 150)
                        {
                            b.SetPixel(i, j, c);
                        }
                        else
                        {
                            b.SetPixel(i, j, actualC);
                        }
                    }

                }
                spriteMap.Add(agentType + c.Name, b);
            }
            else
            {
                b = spriteMap[agentType + c.Name];
            }


            return b;
        }

        private void DrawToBitMap()
        {
            Bitmap bitmap = new Bitmap(600, 600, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                Bitmap img = spriteMap["Tile"];
                foreach (Patch p in es.sys.patches)
                {

                    Bitmap b = changeColour(img, Color.FromName(((NSString)p.properties.GetProperty("p-color")).val.Replace('\"', ' ').Trim()), "Patch");
                    colorMap[p.x, p.y] = ((NSString)p.properties.GetProperty("p-color")).val.Replace('\"', ' ');
                   
                    g.DrawImageUnscaled(b, new Point(p.x * img.Width, p.y * img.Height));

                }
                img = spriteMap["Agent"];
                foreach (Agent a in es.sys.agents.Values)
                {
                    float dx =  img.Width / 2;
                    float dy =  img.Height/ 2;
                    Bitmap b = changeColour(img, Color.FromName(((NSString)a.properties.GetProperty("color")).val.Replace('\"', ' ').Trim()), "Agent");
               
                    g.DrawImageUnscaled(b, new Point(a.x*img.Width,a.y*img.Height));
                   

                }

                if (data != null)
                {
                    data.Dispose();
                    data = null;
                }

                data = bitmap;
            }

        }

        private void AgentDraw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

#if DRAWBITMAP
            DrawToBitMap();
            if (data != null)
            {
                try
                {
                    g.DrawImageUnscaled(data, new Point(0, 0));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
                
            }
#else

            int i = 0;
            int j = 0;
            Bitmap img = spriteMap["Tile"];
            string color;
            foreach (Patch p in es.sys.patches)
            {
              
                    Bitmap b = changeColour(img, Color.FromName(((NSString)p.properties.GetProperty("p-color")).val.Replace('\"', ' ').Trim()), "Patch");
                    colorMap[p.x, p.y] = ((NSString)p.properties.GetProperty("p-color")).val.Replace('\"', ' ');
                    g.DrawImage(b, new PointF(p.x * img.Width, p.y * img.Height));
                
            }
            img = spriteMap["Agent"];
            foreach (Agent a in es.sys.agents.Values)
            {
                
                Bitmap b = changeColour(img, Color.FromName(((NSString)a.properties.GetProperty("color")).val.Replace('\"', ' ').Trim()),"Agent");
                g.DrawImage(b, new PointF(a.x*img.Width , a.y * img.Height));

            }
#endif
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            exeThread = false;
            ThreadWait();
            OpenFileDialog d = new OpenFileDialog();
            txtScript.Clear();
            d.Multiselect = false;
            if (d.ShowDialog() == DialogResult.OK)
            {
                var v = File.ReadAllLines(d.FileName);
                foreach(string s in v)
                {
                    txtScript.AppendText(s);
                       txtScript.AppendText(Environment.NewLine);
                    
                 }
            }
            scriptVerified = false;

        }

        private void ThreadWait()
        {
            if (t != null && !t.IsAlive)
            {
                t = null;
                if (es.sys.ExceptionThrown)
                {
                    es.sys.ExceptionThrown = false;
                }
            }
            if (t != null && t.IsAlive)
            {
                
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                es.sys.ExceptionThrown = true;
                es.sys.BreakExecution = true;
               


                es = new ExecutionEngine();
                scriptVerified = false;

            }

        }

            private void btbVerify_Click(object sender, EventArgs e)
        {
           
            exeThread = false;
            ThreadWait();
            if (!string.IsNullOrEmpty(txtScript.Text)||!string.IsNullOrWhiteSpace(txtScript.Text))
            {
                txtConsole.Clear();
                es = new ExecutionEngine();
                es.Load(txtScript.Text.Split(Environment.NewLine.ToCharArray()[0]));
                scriptVerified = true;
                pbSim.Refresh();
            }

        }
        private void ThreadRuntime(string input)
        {
            while (exeThread)
            {
                string text = txtInput.Text;
#if CALLTRACK || ALLTRACK
                PeformanceTracker.StartStopWatch(text);
#endif

                try
                {
                    ParseTree ts = new ParseTree(input);
                    es.ExecuteTree(ts);
                }
                catch (Exception e)
                {
                  Console.WriteLine(e.Message);
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine(e.StackTrace);
                }

                if (!InvokeRequired)
                {
                    lblTicks.Text = ((Number)es.sys.Get("ticks")).val.ToString();
                }
                else
                {
                    Invoke(new Action(() => { lblTicks.Text = ((Number)es.sys.Get("ticks")).val.ToString(); }));
                }


                if (!InvokeRequired)
                {
                    pbSim.Refresh();
                }
                else
                {
                    Invoke(new Action(() => { pbSim.Refresh(); }));
                }
#if CALLTRACK || ALLTRACK
                PeformanceTracker.Stop(text);
#endif
            }
        }


        
        private void btbExec_Click(object sender, EventArgs e)
        {
            exeThread = false;
            ThreadWait();
            string input;
            string paramt;
            if (scriptVerified)
            {
                if (txtInput.Text.Contains(" "))
                {
                    input = txtInput.Text.Split(' ')[0];
                    paramt = txtInput.Text.Split(' ')[1];
                    if (paramt == "-forever")
                    {
                        t = new Thread(() => { ThreadRuntime(input); }) {Name  = "Runtime Thread" }; 
                           
                        exeThread = true;

                        t.Start();




                    }
                    else
                    {
#if CALLTRACK || ALLTRACK
            PeformanceTracker.StartStopWatch(txtInput.Text);
#endif
                        ParseTree t = new ParseTree(txtInput.Text);
                        es.ExecuteTree(t);
                        lblTicks.Text = ((Number)es.sys.Get("ticks")).val.ToString();
                        pbSim.Refresh();
#if CALLTRACK || ALLTRACK
                        PeformanceTracker.Stop(txtInput.Text);
#endif

                    }
                }
                else
                {
#if CALLTRACK || ALLTRACK
                    PeformanceTracker.StartStopWatch(txtInput.Text);
#endif
                    ParseTree t = new ParseTree(txtInput.Text);
                    es.ExecuteTree(t);
                    lblTicks.Text = ((Number)es.sys.Get("ticks")).val.ToString();
                    pbSim.Refresh();
#if CALLTRACK || ALLTRACK
                    PeformanceTracker.Stop(txtInput.Text);
#endif
                }

            }
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            exeThread = false;
            ThreadWait();
            SaveFileDialog d = new SaveFileDialog();
            
            if (d.ShowDialog() == DialogResult.OK)
            {
                File.Create(d.FileName).Close();
                File.WriteAllText(d.FileName, txtScript.Text);
            }
            scriptVerified = false;
        }

        private void newToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            exeThread = false;
            ThreadWait();
            es = new ExecutionEngine();
            txtScript.Clear();
            
        }

        private void pbSim_Click(object sender, EventArgs e)
        {

        }
    }
}
