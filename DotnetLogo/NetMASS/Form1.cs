using NParser.Runtime;
using NParser.Runtime.DataStructs;
using NParser.Types;
using NParser.Types.Agents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private string[,] colorMap;
        private bool exeThread = false;
        bool scriptVerified = false;
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


        private void AgentDraw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            

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
            OpenFileDialog d = new OpenFileDialog();
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

        private void btbVerify_Click(object sender, EventArgs e)
        {

            exeThread = false;
            if (!string.IsNullOrEmpty(txtScript.Text)||!string.IsNullOrWhiteSpace(txtScript.Text))
            {
                txtConsole.Clear();
                es = new ExecutionEngine();
                es.Load(txtScript.Text.Split(Environment.NewLine.ToCharArray()[0]));
                scriptVerified = true;
                pbSim.Refresh();
            }

        }

        private void btbExec_Click(object sender, EventArgs e)
        {
            exeThread = false;
            string input;
            string paramt;
            if (scriptVerified)
            {
                if (txtInput.Text.Contains(" "))
                {
                    input = txtInput.Text.Split(' ')[0];
                    paramt = txtInput.Text.Split(' ')[1];
                    if (paramt == "-exeinfinite")
                    {
                        Thread t = new Thread(() =>
                        {
                            while (exeThread)
                            {
                                ParseTree ts = new ParseTree(input);
                                es.ExecuteTree(ts);
                                lblTicks.Text = ((Number)es.sys.Get("ticks")).val.ToString();
                                if (!InvokeRequired)
                                {
                                    pbSim.Refresh();
                                }
                                else
                                {
                                    Invoke(new Action(() => { pbSim.Refresh(); }));
                                }
                            }
                        });
                        exeThread = true;

                        t.Start();




                    }
                    else
                    {
                        ParseTree t = new ParseTree(txtInput.Text);
                        es.ExecuteTree(t);
                        lblTicks.Text = ((Number)es.sys.Get("ticks")).val.ToString();
                        pbSim.Refresh();

                    }
                }
                else
                {
                    ParseTree t = new ParseTree(txtInput.Text);
                    es.ExecuteTree(t);
                    lblTicks.Text = ((Number)es.sys.Get("ticks")).val.ToString();
                    pbSim.Refresh();
                }

            }
        }
    }
}
